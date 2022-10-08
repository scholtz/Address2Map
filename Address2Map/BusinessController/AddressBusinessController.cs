using Address2Map.Model;
using Address2Map.Repository;
using System.Text;
using System.Text.RegularExpressions;

namespace Address2Map.BusinessController
{
    /// <summary>
    /// Business logic of the address controller
    /// </summary>
    public class AddressBusinessController
    {
        private readonly RuianRepository ruianRepository;
        private const string DashPattern = @"[\u2012\u2013\u2014\u2015]";
        private static Regex _dashRegex = new Regex(DashPattern);
        private const string ValidityPattern = @"^([^-]+?)( - (lichá č.|sudá č.|č.|č. p.)( (\d+ ?[–-] ?\d+|(od )?\d+( a)? výše|\d+)((, ?| ?a ?)(\d+ ?[–-] ?\d+|(od )?\d+( a)? výše|\d+))*)?((, ?| ?a ?)(lichá č.|sudá č.|č.|č. p.)( (\d+ ?[–-] ?\d+|(od )?\d+( a)? výše|\d+)((, ?| ?a ?)(\d+ ?[–-] ?\d+|(od )?\d+( a)? výše|\d+))*)?)*)?$";
        private static Regex ValidityPatternRegex = new Regex(ValidityPattern);
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ruianRepository"></param>
        public AddressBusinessController(RuianRepository ruianRepository)
        {
            this.ruianRepository = ruianRepository;
        }
        /// <summary>
        /// Convert text 2 output
        /// </summary>
        /// <returns></returns>
        public TextConversion ProcessText2Output(uint city, string input)
        {
            var instr = new StringBuilder();
            var outstr = new StringBuilder();
            var notestr = new StringBuilder();

            foreach (var item in input.Split('\n'))
            {
                (var outStr, var noteStr, var dataPoints) = ProcessLine(city, item.Trim(), false);
                instr.AppendLine(item);
                outstr.AppendLine(outStr);
                notestr.AppendLine(noteStr);
            }

            return new TextConversion()
            {
                Input = instr.ToString(),
                Notes = notestr.ToString(),
                Output = outstr.ToString(),
            };
        }
        /// <summary>
        /// Convert text 2 output
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DataPoint> ProcessText2DataPoints(uint city, string input)
        {
            var ret = new List<DataPoint>();
            foreach (var item in input.Split('\n'))
            {
                (var outStr, var noteStr, var dataPoints) = ProcessLine(city, item.Trim(), true);
                if (dataPoints?.Any() == true)
                {
                    ret.AddRange(dataPoints);
                }
            }

            return ret;
        }
        /// <summary>
        /// Process line in input with the processing rules
        /// </summary>
        /// <param name="city"></param>
        /// <param name="line"></param>
        /// <param name="processDataPoints"></param>
        /// <returns></returns>
        public (string outStr, string noteStr, IEnumerable<DataPoint> dataPoints) ProcessLine(uint city, string line, bool processDataPoints)
        {
            var err = "";

            var dataPoints = new List<DataPoint>();

            if (_dashRegex.IsMatch(line))
            {
                err += "UTF Dash has been replaced with hyphen";
                line = _dashRegex.Replace(line, "-");
            }
#if REGEXWorks
            if (!ValidityPatternRegex.IsMatch(line))
            {
                if (!string.IsNullOrEmpty(err)) err += "; ";
                err += $"Format of the input is incorrect";
            }
#endif

            var posEndStreet = line.IndexOf(" -");
            var street = line;
            if (posEndStreet >= 0)
            {
                street = line.Substring(0, posEndStreet);
            }

            // check if street is valid
            var suggestion = ruianRepository.SuggestStreet(city, street);
            if (suggestion != null && suggestion.Name != street)
            {
                if (!string.IsNullOrEmpty(err)) err += "; ";
                err += "We suggest to change street name";
                line = line.Replace(street, suggestion.Name);
            }
            if (suggestion == null)
            {
                if (!string.IsNullOrEmpty(err)) err += "; ";
                err += $"We had trouble finding street {street}";
            }

            if (suggestion?.Name == street)
            {
                if (processDataPoints)
                {
                    dataPoints.AddRange(ruianRepository.GetStreetDataPoints(suggestion.Code));
                }
            }

            return (line, err, dataPoints);
        }

        internal IEnumerable<Model.City> AutocompleteCity(string cityName)
        {
            return ruianRepository.AutocompleteCity(cityName);
        }

        internal IEnumerable<Street> AutocompleteStreet(uint cityCode, string streetName)
        {
            return ruianRepository.AutocompleteStreet(cityCode, streetName);
        }
    }
}
