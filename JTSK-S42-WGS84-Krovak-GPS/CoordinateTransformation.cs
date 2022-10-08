using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace JTSK_S42_WGS84_Krovak_GPS
{
    /// <summary>
    /// Statická třída pro podporu práce s geografickými souřadnicemi.
    /// </summary>
    /// <remarks>
    /// Základní pojmy viz. http://www.gis.zcu.cz/studium/gen1/html/ch02s03.html
    /// 
    /// Original creator: https://github.com/xPaRi/ShowInMap/blob/master/MyCoordinateTransformation.cs
    /// </remarks>
    public static class CoordinateTransformation
    {
        /// <summary>
        /// Převede zeměpisnou souřadnici v systému DegDec na Stupně.
        /// </summary>
        /// <param name="value">Zeměpisná souřadnice DegDec.</param>
        /// <returns>Stupně.</returns>
        public static int DecToDegrees(double value)
        {
            return new AngleDeg(value).Degrees;
        }

        /// <summary>
        /// Převede zeměpisnou souřadnici v systému DegDec na Minuty.
        /// </summary>
        /// <param name="value">Zeměpisná souřadnice DegDec.</param>
        /// <returns>Minuty.</returns>
        public static int DecToMinutes(double value)
        {
            return new AngleDeg(value).Minutes;
        }

        /// <summary>
        /// Převede zeměpisnou souřadnici v systému DegDec na Vteřiny.
        /// </summary>
        /// <param name="value">Zeměpisná souřadnice DegDec.</param>
        /// <returns>Vteřiny.</returns>
        public static double DecToSeconds(double value)
        {
            return new AngleDeg(value).Seconds;
        }

        /// <summary>
        /// Převede zeměpisnou souřadnici ve stupních na systém DegDec.
        /// </summary>
        /// <param name="degrees">Stupně.</param>
        /// <param name="minutes">Minuty.</param>
        /// <param name="seconds">Vteřiny.</param>
        /// <returns>Zeměpisná souřadnice v systému Dec.</returns>
        public static double DegreesToDec(double degrees, double minutes, double seconds)
        {
            var angleDeg = new AngleDeg(Convert.ToInt32(degrees), Convert.ToInt32(minutes), seconds);

            return angleDeg.ToDec();
        }

        /// <summary>
        /// Převede zadanou souřadnici na řetězec Stupně, minuty, vteřiny.
        /// </summary>
        /// <param name="value">Zeměpisná souřadnice.</param>
        /// <returns>Stupně, minuty, vteřiny.</returns>
        public static string ToDegString(double? value)
        {
            return ToDegString(System.Globalization.CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Převede zadanou souřadnici na řetězec Stupně, minuty, vteřiny.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="value">Zeměpisná souřadnice.</param>
        /// <returns>Stupně, minuty, vteřiny.</returns>
        public static string ToDegString(IFormatProvider provider, double? value)
        {
            if (value.HasValue)
                return new AngleDeg(value.Value).ToString(provider);

            return string.Format(AngleDeg.FormatString, "?", "?", "?");
        }

        /// <summary>
        /// Převede zadanou souřadnici na řetězec Stupně, minuty, vteřiny.
        /// </summary>
        /// <param name="latitude">Zeměpisná šířka.</param>
        /// <param name="longitude">Zeměpisná délka.</param>
        /// <returns>Stupně, minuty, vteřiny.</returns>
        public static string ToDegString(double? latitude, double? longitude)
        {
            string latitudeAbbrev;
            string longitudeAbbrev;

            //--- určení délek a šířek
            if (latitude.HasValue)
                latitudeAbbrev = (latitude >= 0) ? "sš" : "jš";
            else
                latitudeAbbrev = string.Empty;

            if (longitude.HasValue)
                longitudeAbbrev = (longitude >= 0) ? "vd" : "zd";
            else
                longitudeAbbrev = string.Empty;
            //---

            return $"{ToDegString(latitude)} {latitudeAbbrev}; {ToDegString(longitude)} {longitudeAbbrev}";
        }


    }



    
   
}