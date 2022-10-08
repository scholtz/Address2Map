using Address2Map.Model;
using JTSK_S42_WGS84_Krovak_GPS;
using Microsoft.VisualBasic.FileIO;
using Slugify;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text;

namespace Address2Map.Repository
{
    public class RuianRepository
    {
        ConcurrentDictionary<uint, City> cityCode2City = new ConcurrentDictionary<uint, City>();
        //ConcurrentDictionary<uint, Street> streetCode2Street = new ConcurrentDictionary<uint, Street>();
        ConcurrentDictionary<uint, ConcurrentDictionary<uint, Street>> cityCode2Streets = new ConcurrentDictionary<uint, ConcurrentDictionary<uint, Street>>();
        ConcurrentDictionary<uint, ConcurrentDictionary<uint, DataPoint>> street2DataPoint = new ConcurrentDictionary<uint, ConcurrentDictionary<uint, DataPoint>>();
        private readonly SlugHelper slugHelper;
        /// <summary>
        /// Constructor
        /// </summary>
        public RuianRepository()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var slugConfig = new SlugHelperConfiguration();
            slugConfig.TrimWhitespace = true;
            slugConfig.ForceLowerCase = true;
            slugConfig.StringReplacements.Add(".", "");
            slugHelper = new SlugHelper(slugConfig);
        }

        const int FieldsCount = 19;
        const int ColAxisY = 16;
        const int ColAxisX = 17;
        const int ColAdmCode = 0;
        const int ColCityCode = 1;
        const int ColCityName = 2;
        const int ColAreaCode = 7;
        const int ColAreaName = 8;
        const int ColStreetCode = 9;
        const int ColStreetName = 10;
        const int ColStreetPart1 = 11;
        const int ColStreetPartNum1 = 12;
        const int ColStreetPartNum2 = 13;

        /// <summary>
        /// Process single csv file of administrative unit points
        /// </summary>
        /// <param name="csv"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public int ProcessCSV(byte[] csv)
        {
            int ret = 0;
            Encoding ascii = Encoding.GetEncoding("Windows-1250");
            byte[] utf8Bytes = Encoding.Convert(ascii, Encoding.UTF8, csv);
            using var data = new MemoryStream(utf8Bytes);

            using TextFieldParser csvParser = new TextFieldParser(data);
            if (csvParser == null) throw new Exception("Error occured parsing csv");
            csvParser.CommentTokens = new string[] { "#" };
            csvParser.SetDelimiters(new string[] { ";" });
            csvParser.HasFieldsEnclosedInQuotes = true;

            var headers = csvParser?.ReadFields()?.Select(i => slugHelper.GenerateSlug(i)).ToArray();
            if (headers?.Length != FieldsCount) throw new Exception("CSV file is in wrong format. Headers count does not match");
            if (headers[0] != "kod-adm") throw new Exception("Wrong header. Incorrect first header value");
            if (headers[ColAxisY] != "souradnice-y") throw new Exception("Wrong header. Incorrect AxisY");
            if (headers[ColAxisX] != "souradnice-x") throw new Exception("Wrong header. Incorrect AxisX");
            if (headers[ColCityCode] != "kod-obce") throw new Exception("Wrong header. Kód obce");

            while (!csvParser.EndOfData)
            {
                var fields = csvParser.ReadFields();
                if (fields?.Any() != true) continue;
                if (fields?.Length != FieldsCount) throw new Exception("CSV file is in wrong format. Number of columns in data row is wrong");
                // Read current line fields, pointer moves to the next line.
                uint streetCode = 0;
                if (fields[ColStreetCode] == "")
                {
                    //continue;// Kunratice cp 53

                    if (string.IsNullOrEmpty(fields[ColAreaCode])) continue;
                    streetCode = uint.Parse(fields[ColAreaCode]) + 1000000000;
                }
                else
                {
                    streetCode = uint.Parse(fields[ColStreetCode]);
                }
                var dataPoint = MakeDataPoint(fields);
                if (dataPoint == null) continue;
                var cityCode = uint.Parse(fields[ColCityCode]);
                var admCode = uint.Parse(fields[ColAdmCode]);
                if(fields[ColStreetName] == "Zlatá ulička u Daliborky")
                {

                }
                if (!cityCode2City.ContainsKey(cityCode)) cityCode2City[cityCode] = new City() { Code = cityCode, Name = fields[ColCityName], Slug = slugHelper.GenerateSlug(fields[ColCityName]) };
                var street = new Street() { Code = streetCode, Name = fields[ColStreetName], Slug = slugHelper.GenerateSlug(fields[ColStreetName]) };
                if (!cityCode2Streets.ContainsKey(cityCode)) cityCode2Streets[cityCode] = new ConcurrentDictionary<uint, Street>();
                if (!cityCode2Streets[cityCode].ContainsKey(streetCode))
                {
                    cityCode2Streets[cityCode][streetCode] = street;
                    ret++;
                }

                if (!street2DataPoint.ContainsKey(streetCode)) street2DataPoint[streetCode] = new ConcurrentDictionary<uint, DataPoint>();

                street2DataPoint[streetCode][admCode] = dataPoint;

            }

            return ret;
        }

        private DataPoint? MakeDataPoint(string[] fields)
        {
            var ret = new DataPoint();

            var street = fields[ColStreetName];
            var streetSuffix = fields[ColStreetPart1];
            var streetNum1 = fields[ColStreetPartNum1];
            var streetNum2 = fields[ColStreetPartNum2];

            if (string.IsNullOrEmpty(street)) street = fields[ColStreetName];

            ret.Address = street;
            if (!string.IsNullOrEmpty(streetSuffix)) ret.Address += $" {streetSuffix}";
            if (!string.IsNullOrEmpty(streetNum1)) ret.Address += $" {streetNum1}";
            if (!string.IsNullOrEmpty(streetNum1) && !string.IsNullOrEmpty(streetNum2)) ret.Address += $"/";
            if (!string.IsNullOrEmpty(streetNum2)) ret.Address += $"{streetNum2}";

            if (!decimal.TryParse(fields[ColAxisX], NumberStyles.Any, CultureInfo.InvariantCulture, out var x)) return null;
            if (!decimal.TryParse(fields[ColAxisY], NumberStyles.Any, CultureInfo.InvariantCulture, out var y)) return null;
            var wgs84 = new JTSK2065Coordinate(Convert.ToDouble(x), Convert.ToDouble(y)).WGS84Coordinate;
            ret.Lat = Convert.ToDecimal(wgs84.LatitudeDec);
            ret.Lng = Convert.ToDecimal(wgs84.LongitudeDec);

            return ret;
        }
        /// <summary>
        /// Returns the same input if it was found
        /// Returns corrected input if it was able to find alternative
        /// Returns empty string if not found
        /// 
        /// </summary>
        /// <param name="city"></param>
        /// <param name="street"></param>
        /// <returns></returns>
        internal Street? SuggestStreet(uint city, string street)
        {
            if (!cityCode2Streets.ContainsKey(city)) return null;
            var findExact = cityCode2Streets[city].Values.FirstOrDefault(i => i.Name == street);
            if (findExact != null)
            {
                return findExact;
            }
            var slug = slugHelper.GenerateSlug(street);
            var findSlug = cityCode2Streets[city].Values.FirstOrDefault(i => i.Slug == slug);
            if (findSlug != null)
            {
                return findSlug;
            }
            var slugNabrezi = $"{slug}ezi";
            findSlug = cityCode2Streets[city].Values.FirstOrDefault(i => i.Slug == slugNabrezi);

            return findSlug; // found or null
        }

        internal IEnumerable<City> AutocompleteCity(string cityName)
        {
            var slug = slugHelper.GenerateSlug(cityName);
            return cityCode2City.Values.Where(c => c.Slug.StartsWith(slug)).OrderBy(k => k.Name);
        }

        internal IEnumerable<Street> AutocompleteStreet(uint cityCode, string streetName)
        {
            var slug = slugHelper.GenerateSlug(streetName);
            if (!cityCode2Streets.ContainsKey(cityCode)) return Enumerable.Empty<Street>();
            return cityCode2Streets[cityCode].Values.Where(c => c.Slug.StartsWith(streetName)).OrderBy(k => k.Name);
        }

        internal IEnumerable<DataPoint> GetStreetDataPoints(uint streetCode)
        {
            if (!street2DataPoint.ContainsKey(streetCode)) return Enumerable.Empty<DataPoint>();
            // todo add filter by 
            return street2DataPoint[streetCode].Values;
        }
    }
}
