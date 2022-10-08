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
                (var outStr, var noteStr) = ProcessLine(city, item.Trim());
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
        /// Process line in input with the processing rules
        /// </summary>
        /// <param name="city"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public (string outStr, string noteStr) ProcessLine(uint city,string line)
        {
            var err = "";

            
            if (_dashRegex.IsMatch(line))
            {
                err += "UTF Dash has been replaced with hyphen";
                line = _dashRegex.Replace(line, "-");
            }

            var posEndStreet = line.IndexOf(" -");
            var street = line;
            if (posEndStreet >= 0)
            {
                street = line.Substring(0, posEndStreet); 
            }

            // check if street is valid
            var suggestion = ruianRepository.SuggestStreet(city, street);
            if(suggestion != null && suggestion != street)
            {
                if (!string.IsNullOrEmpty(err)) err += "; ";
                err += "We suggest to change street name";
                line = line.Replace(street, suggestion);
            }
            

            return (line, err);
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
