using Address2Map.Model;
using Address2Map.Repository;
using System.Data.SqlTypes;
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
        private const string ValidityPattern = @"^([^-]+?)( - (lichá č.|sudá č.|č.|č. p.)( (\d+ ?[–-] ?\d+|(od )?\d+( a)? výše|\d+)((, ?| a )(\d+ ?[–-] ?\d+|(od )?\d+( a)? výše|\d+))*)?((, ?| a )(lichá č.|sudá č.|č.|č. p.)( (\d+ ?[–-] ?\d+|(od )?\d+( a)? výše|\d+)((, ?| a )(\d+ ?[–-] ?\d+|(od )?\d+( a)? výše|\d+))*)?)*)?$";
        private static Regex ValidityPatternRegex = new Regex(ValidityPattern);
        private static RegexOptions options = RegexOptions.None;
        private static Regex DoubleSpacesRegex = new Regex("[ ]{2,}", options);
        private static Regex StreetSeriesTypeRegex = new Regex(@"(lichá č.|sudá č.|č. p.|č.) ");
        private static Regex RangeRegex = new Regex(@"(\d+ ?[–-] ?\d+|(od )?\d+( a)? výše|\d+)(, ?| a )");
        private static Regex NumberRegex = new Regex(@"\d+");

        private const string OddTypeString = "lichá č. ";
        private const string EvenTypeString = "sudá č. ";
        private const string AllTypeString = "č. ";
        private const string CPTypeString = "č. p. ";

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
        public IEnumerable<IEnumerable<DataPoint>> ProcessText2DataPoints(uint city, string input)
        {
            var ret = new List<List<DataPoint>>();
            var currentList = new List<DataPoint>();
            foreach (var item in input.Split('\n'))
            {
                if (item.StartsWith("!"))
                {
                    if(currentList.Count > 0)
                    {
                        ret.Add(currentList);
                        currentList = new List<DataPoint>();
                    }
                }
                (var outStr, var noteStr, var dataPoints) = ProcessLine(city, item.Trim(), true);
                if (dataPoints?.Any() == true)
                {
                    currentList.AddRange(dataPoints);
                }
            }
            if (currentList.Count > 0)
            {
                ret.Add(currentList);
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

            if (line.Trim().StartsWith("!"))
            {
                return (line, err, dataPoints);
            }
            if (string.IsNullOrEmpty(line))
            {
                return (line, err, dataPoints);
            }

            if (_dashRegex.IsMatch(line))
            {
                err += "UTF Dash has been replaced with hyphen";
                line = _dashRegex.Replace(line, "-");
            }

            // remove double spaces
            line = DoubleSpacesRegex.Replace(line, " ");

            if (!ValidityPatternRegex.IsMatch(line))
            {
                if (!string.IsNullOrEmpty(err)) err += "; ";
                err += $"Format of the input is incorrect";

                return (line, err, dataPoints);
            }

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

            if (posEndStreet >= 0)
            {
                var numberSpecification = line.Substring(posEndStreet + 2).Trim();
                var rules = GetStreetNumberRules(numberSpecification);
            }


            if (suggestion?.Name == street)
            {
                if (processDataPoints)
                {
                    dataPoints.AddRange(ruianRepository.GetStreetDataPoints(suggestion.Code, rules));
                }
            }

            return (line, err, dataPoints);
        }

        public IEnumerable<StreetNumberRule> GetStreetNumberRules(string numberSpecification)
        {
            var rules = new List<StreetNumberRule>();
            var position = 0;
            numberSpecification = numberSpecification + " ";

            while (position < numberSpecification.Length)
            {
                var remainder = numberSpecification.Substring(position);
                var match = StreetSeriesTypeRegex.Match(remainder);
                // if no series type is found, it's an error
                if (!match.Success || match.Index != 0)
                {
                    return null;
                }
                var type = GetSeriesType(match.Value);
                position += match.Length;
                remainder = numberSpecification.Substring(position);
                match = StreetSeriesTypeRegex.Match(remainder);
                var end = numberSpecification.Length;
                if (match.Success)
                {
                    end = position + match.Index;
                }
                var rangePart = numberSpecification.Substring(position, end - position);
                rules.AddRange(GetRulesFromRangePart(rangePart, type));
                
                position = end;
            }

            return rules;
        }

        internal IEnumerable<StreetNumberRule> GetRulesFromRangePart(string rangePart, StreetNumberSeriesType seriesType)
        {
            var rules = new List<StreetNumberRule>();

            if (rangePart.Trim() == "")
            {
                rules.Add(new StreetNumberRule{ SeriesType = seriesType });
                return rules;
            }

            int position = 0;
            rangePart = rangePart.Trim() + ", ";
            while (position < rangePart.Length)
            {
                var match = RangeRegex.Match(rangePart.Substring(position));
                if (!match.Success)
                {
                    break;
                }
                position += match.Index + match.Length;

                var value = RemoveSeparatorFromEnd(match.Value);
                if (value.IndexOf("-") != -1)
                {
                    var fromTo = value.Split("-");
                    rules.Add(new StreetNumberRule{ From = (uint)Int32.Parse(fromTo[0]), To = (uint)Int32.Parse(fromTo[1]), SeriesType = seriesType });
                }
                else if (value.IndexOf("výše") != -1)
                {
                    var number = (uint)Int32.Parse(NumberRegex.Match(value).Value);
                    rules.Add(new StreetNumberRule { From = number, SeriesType = seriesType });
                }
                else
                {
                    var number = (uint)Int32.Parse(NumberRegex.Match(value).Value);
                    rules.Add(new StreetNumberRule { From = number, To = number, SeriesType = seriesType });
                }
            }

            return rules;
        }

        internal StreetNumberSeriesType GetSeriesType(string pattern)
        {
            switch (pattern)
            {
                case OddTypeString: return StreetNumberSeriesType.Odd;
                case EvenTypeString: return StreetNumberSeriesType.Even;
                case CPTypeString: return StreetNumberSeriesType.CP;
                case AllTypeString:
                default: return StreetNumberSeriesType.All;
            }
        }

        internal string RemoveSeparatorFromEnd(string text)
        {
            text = text.TrimEnd();
            if (text[text.Length - 1] == 'a' || text[text.Length - 1] == ',')
            {
                text = text.Substring(0, text.Length - 1);
            }
            return text.TrimEnd();
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
