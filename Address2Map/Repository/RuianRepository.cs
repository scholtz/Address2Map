using Address2Map.Model;
using Microsoft.VisualBasic.FileIO;
using Slugify;
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace Address2Map.Repository
{
    public class RuianRepository
    {
        ConcurrentDictionary<uint, City> cityCode2City = new ConcurrentDictionary<uint, City>();
        //ConcurrentDictionary<uint, Street> streetCode2Street = new ConcurrentDictionary<uint, Street>();
        ConcurrentDictionary<uint, HashSet<Street>> cityCode2Streets = new ConcurrentDictionary<uint, HashSet<Street>>();
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
        const int ColCityCode = 1;
        const int ColCityName = 2;
        const int ColStreetCode = 9;
        const int ColStreetName = 10;

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
                if (fields[ColStreetCode] == "") continue;// Kunratice cp 53
                var cityCode = uint.Parse(fields[ColCityCode]);

                var streetCode = uint.Parse(fields[ColStreetCode]);
                if (!cityCode2City.ContainsKey(cityCode)) cityCode2City[cityCode] = new City() { Code = cityCode, Name = fields[ColCityName] };
                var street = new Street() { Code = streetCode, Name = fields[ColStreetName], Slug = slugHelper.GenerateSlug(fields[ColStreetName]) };
                if (!cityCode2Streets.ContainsKey(cityCode)) cityCode2Streets[cityCode] = new HashSet<Street>();
                if (!cityCode2Streets[cityCode].Contains(street))
                {
                    cityCode2Streets[cityCode].Add(street);
                    ret++;
                }
            }

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
        internal string? SuggestStreet(uint city, string street)
        {
            if (!cityCode2Streets.ContainsKey(city)) return null;
            var findExact = cityCode2Streets[city].FirstOrDefault(i => i.Name == street);
            if (findExact != null)
            {
                return street;
            }
            var slug = slugHelper.GenerateSlug(street);
            var findSlug = cityCode2Streets[city].FirstOrDefault(i => i.Slug == slug);
            if (findSlug != null)
            {
                return findSlug.Name;
            }
            var slugNabrezi = $"{slug}ezi";
            findSlug = cityCode2Streets[city].FirstOrDefault(i => i.Slug == slugNabrezi);
            if (findSlug != null)
            {
                return findSlug.Name;
            }

            return findSlug?.Name;
        }
    }
}
