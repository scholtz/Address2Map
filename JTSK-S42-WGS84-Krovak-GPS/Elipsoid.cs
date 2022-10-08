using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTSK_S42_WGS84_Krovak_GPS
{
    #region Elipsoid

    /// <summary>
    /// Elipsoid a jeho parametry.
    /// </summary>
    public class Elipsoid
    {
        public readonly string Name;
        public readonly double A;
        public readonly double EccSquared;

        public Elipsoid(string name, double a, double eccSquared)
        {
            Name = name;
            A = a;
            EccSquared = eccSquared;
        }

        public static Elipsoid GetElipsoid(string datumName)
        {
            switch (datumName.ToUpper())
            {
                case "AIRY": return new Elipsoid("Airy", 6377563, 0.00667054);
                case "AUSTRALIAN NATIONAL": return new Elipsoid("Australian National", 6378160, 0.006694542);
                case "BESSEL 1841": return new Elipsoid("Bessel 1841", 6377397, 0.006674372);
                case "BESSEL 1841 NAMBIA": return new Elipsoid("Bessel 1841 Nambia", 6377484, 0.006674372);
                case "CLARKE 1866": return new Elipsoid("Clarke 1866", 6378206, 0.006768658);
                case "CLARKE 1880": return new Elipsoid("Clarke 1880", 6378249, 0.006803511);
                case "EVEREST": return new Elipsoid("Everest", 6377276, 0.006637847);
                case "FISCHER 1960 MERCURY": return new Elipsoid("Fischer 1960 Mercury", 6378166, 0.006693422);
                case "FISCHER 1968": return new Elipsoid("Fischer 1968", 6378150, 0.006693422);
                case "GRS 1967": return new Elipsoid("GRS 1967", 6378160, 0.006694605);
                case "GRS 1980": return new Elipsoid("GRS 1980", 6378137, 0.00669438);
                case "HELMERT 1906": return new Elipsoid("Helmert 1906", 6378200, 0.006693422);
                case "HOUGH": return new Elipsoid("Hough", 6378270, 0.00672267);
                case "INTERNATIONAL": return new Elipsoid("International", 6378388, 0.00672267);
                case "KRASSOVSKY": return new Elipsoid("Krassovsky", 6378245, 0.006693422);
                case "MODIFIED AIRY": return new Elipsoid("Modified Airy", 6377340, 0.00667054);
                case "MODIFIED EVEREST": return new Elipsoid("Modified Everest", 6377304, 0.006637847);
                case "MODIFIED FISCHER 1960": return new Elipsoid("Modified Fischer 1960", 6378155, 0.006693422);
                case "SOUTH AMERICAN 1969": return new Elipsoid("South American 1969", 6378160, 0.006694542);
                case "WGS 60": return new Elipsoid("WGS 60", 6378165, 0.006693422);
                case "WGS 66": return new Elipsoid("WGS 66", 6378145, 0.006694542);
                case "WGS 72": return new Elipsoid("WGS 72", 6378135, 0.006694318);
                case "ED50": return new Elipsoid("ED50", 6378388, 0.00672267); // International Ellipsoid
                case "WGS 84": return new Elipsoid("WGS 84", 6378137, 0.00669438);
                case "EUREF89": return new Elipsoid("EUREF89", 6378137, 0.00669438); // Max deviation from WGS 84 is 40 cm/km see http://ocq.dk/euref89 (in danish)
                case "ETRS89": return new Elipsoid("ETRS89", 6378137, 0.00669438); // Same as EUREF89 
                default: throw new Exception($"Neexistující elipsoid (datum) [{datumName}]");
            }
        }

        private static Elipsoid _DefaultElipsoid;

        /// <summary>
        /// Vrací standarní elipsoid (WGS 84).
        /// </summary>
        /// <returns>Elispoid WGS 84.</returns>
        public static Elipsoid GetDefault()
        {
            return _DefaultElipsoid ?? (_DefaultElipsoid = GetElipsoid("WGS 84"));
        }
    }


    #endregion //Elipsoid
}
