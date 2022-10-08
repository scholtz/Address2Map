using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTSK_S42_WGS84_Krovak_GPS
{

    /// <summary>
    /// Souřadnice UTM (Universal Transverse Mercator coordinate system)
    /// </summary>
    public class UTMCoordinate
    {
        public double Easting { get; set; }
        public double UTMEasting { get; set; }
        public double Northing { get; set; }
        public double UTMNorthing { get; set; }
        public int ZoneNumber { get; set; }
        public string ZoneLetter { get; set; }
        public string Zona => $"{ZoneNumber}{ZoneLetter}";

        //Něco tady nehraje, tak to zatím nechávám tak...
        //public UTMCoordinate(double easting, double northing, int zoneNumber, string zoneLetter)
        //{
        //    //Easting = utmEasting, Northing = utmNorthing, ZoneNumber = zoneNumber, ZoneLetter = utmZone
        //}

        public override string ToString()
        {
            return $"{Zona} {Easting}{Northing}";
        }

        /// <summary>
        /// Vrací písmeno zóny za základě zeměpisné délky.
        /// </summary>
        /// <param name="latitude">Zeměpisná délka.</param>
        /// <returns></returns>
        public static string GetUtmZoneLetter(double latitude)
        {
            if (84 >= latitude && latitude >= 72) return "X";
            if (72 > latitude && latitude >= 64) return "W";
            if (64 > latitude && latitude >= 56) return "V";
            if (56 > latitude && latitude >= 48) return "U";
            if (48 > latitude && latitude >= 40) return "T";
            if (40 > latitude && latitude >= 32) return "S";
            if (32 > latitude && latitude >= 24) return "R";
            if (24 > latitude && latitude >= 16) return "Q";
            if (16 > latitude && latitude >= 8) return "P";
            if (8 > latitude && latitude >= 0) return "N";
            if (0 > latitude && latitude >= -8) return "M";
            if (-8 > latitude && latitude >= -16) return "L";
            if (-16 > latitude && latitude >= -24) return "K";
            if (-24 > latitude && latitude >= -32) return "J";
            if (-32 > latitude && latitude >= -40) return "H";
            if (-40 > latitude && latitude >= -48) return "G";
            if (-48 > latitude && latitude >= -56) return "F";
            if (-56 > latitude && latitude >= -64) return "E";
            if (-64 > latitude && latitude >= -72) return "D";
            if (-72 > latitude && latitude >= -80) return "C";

            return "Z";
        }

        /// <summary>
        /// Vrací číslo UTM zóny na základě zeměpisné polohy.
        /// </summary>
        /// <param name="latitude">Zeměpisná délka.</param>
        /// <param name="longitude">Zeměpisná šířka.</param>
        /// <returns></returns>
        public static int GetUtmZoneNumber(double latitude, double longitude)
        {
            if (longitude >= 8 && longitude <= 13 && latitude > 54.5 && latitude < 58)
                return 32;

            if (latitude >= 56.0 && latitude < 64.0 && longitude >= 3.0 && longitude < 12.0)
                return 32;

            if (latitude >= 72.0 && latitude < 84.0)
            {
                if (longitude >= 0.0 && longitude < 9.0)
                    return 31;

                if (longitude >= 9.0 && longitude < 21.0)
                    return 33;

                if (longitude >= 21.0 && longitude < 33.0)
                    return 35;

                if (longitude >= 33.0 && longitude < 42.0)
                    return 37;
            }

            return (int)((longitude + 180) / 6) + 1;
        }

        /// <summary>
        /// Vrací číslo UTM zóny na základě zeměpisné polohy.
        /// </summary>
        /// <param name="wgs84Coordinate">Zeměpisná poloha.</param>
        /// <returns></returns>
        public static int GetUtmZoneNumber(WGS84Coordinate wgs84Coordinate)
        {
            return GetUtmZoneNumber(wgs84Coordinate.LatitudeDec, wgs84Coordinate.LongitudeDec);
        }
    }
}
