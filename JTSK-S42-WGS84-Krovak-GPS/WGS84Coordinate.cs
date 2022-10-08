using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JTSK_S42_WGS84_Krovak_GPS
{
    #region Souřadnicové systémy

    /// <summary>
    /// Souřadnicový systém WGS 84.
    /// </summary>
    /// <remarks>
    /// Jedná se o vojenský souřadnicový systém používaný státy NATO. Referenční plochou je 
    /// elipsoid WGS 84 (World Geodetic System). Použité kartografické zobrazení se 
    /// nazývá UTM (Univerzální transverzální Mercatorovo). 
    /// Systém má počátek v hmotném středu Země (s přesností cca 2 m) – jedná se o geocentrický systém. 
    /// Osa Z je totožná s osou rotace Země v roce 1984. Osy X a Y leží v rovině rovníku. 
    /// Počátek a orientace jeho os X,Y,Z jsou realizovány pomocí 12 pozemských stanic se známými 
    /// přesnými souřadnicemi, které nepřetržitě monitorují dráhy družic systému GPS-NAVSTAR.
    /// </remarks>
    public class WGS84Coordinate
    {

        #region Fields

        /// <summary>
        /// Konstanta pro konverzi z DEG do RAD.
        /// </summary>
        private const double DEG_TO_RAD = Math.PI / 180d; //0.0174532925199432958;

        private double _LatitudeDec;
        private double _LongitudeDec;

        private double _LatitudeRad;
        private double _LongitudeRad;

        #endregion //Fields

        /// <summary>
        /// Vzdálenost [metry] dvou bodů zadaných geografickými souřadnicemi.
        /// </summary>
        /// <remarks>
        /// Souhrný článek o výpočtech vzdálenosti bodů:
        /// https://www.movable-type.co.uk/scripts/latlong.html
        /// </remarks>
        /// <param name="latitude1">Zeměpisná šířka v DegDec bodu 1.</param>
        /// <param name="longitude1">Zeměpisná délka v DegDec bodu 1.</param>
        /// <param name="latitude2">Zeměpisná šířka v DegDec bodu 2.</param>
        /// <param name="longitude2">Zeměpisná délka v DegDec bodu 2.</param>
        /// <returns>Vzdálenost v metrech.</returns>
        public static double GetDistance(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            return GetDistanceByElipsoid(latitude1, longitude1, latitude2, longitude2);
        }


        /// <summary>
        /// Vzdálenost [metry] dvou bodů zadaných geografickými souřadnicemi.
        /// Odchylka max. 0.00055 m.
        /// </summary>
        /// <remarks>
        /// https://stackoverflow.com/questions/6544286/calculate-distance-of-two-geo-points-in-km-c-sharp
        /// </remarks>
        /// <param name="latitude1">Zeměpisná šířka v DegDec bodu 1.</param>
        /// <param name="longitude1">Zeměpisná délka v DegDec bodu 1.</param>
        /// <param name="latitude2">Zeměpisná šířka v DegDec bodu 2.</param>
        /// <param name="longitude2">Zeměpisná délka v DegDec bodu 2.</param>
        /// <returns>Vzdálenost v metrech.</returns>
        public static double GetDistanceByElipsoid(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            long num = 0x615299L;
            double num2 = 6356752.3142;
            double num3 = 0.0033528106647474805;
            double num4 = MyGeo.ToRAD(longitude2 - longitude1);
            double a = Math.Atan((1 - num3) * Math.Tan(MyGeo.ToRAD(latitude1)));
            double num6 = Math.Atan((1 - num3) * Math.Tan(MyGeo.ToRAD(latitude2)));
            double num7 = Math.Sin(a);
            double num8 = Math.Sin(num6);
            double num9 = Math.Cos(a);
            double num10 = Math.Cos(num6);
            double num11 = num4;
            double num12 = 6.2831853071795862;
            int num13 = 20; //počet iterací výpočtu
            double y = 0;
            double x = 0;
            double num18 = 0;
            double num20 = 0;
            double num22 = 0;

            while ((Math.Abs(num11 - num12) > 1E-12) && (--num13 > 0))
            {
                double num14 = Math.Sin(num11);
                double num15 = Math.Cos(num11);

                y = Math.Sqrt((num10 * num14 * num10 * num14) + (((num9 * num8) - (num7 * num10 * num15)) * ((num9 * num8) - (num7 * num10 * num15))));

                if (y == 0)
                    return 0;

                x = (num7 * num8) + (num9 * num10 * num15);
                num18 = Math.Atan2(y, x);
                double num19 = num9 * num10 * num14 / y;
                num20 = 1 - (num19 * num19);
                num22 = (num20 == 0) ? 0 : x - (2 * num7 * num8 / num20);
                double num21 = num3 / 16 * num20 * (4 + (num3 * (4 - (3 * num20))));
                num12 = num11;
                num11 = num4 + ((1 - num21) * num3 * num19 * (num18 + (num21 * y * (num22 + (num21 * x * (-1 + (2 * num22 * num22)))))));
            }

            double num23 = (num20 * ((num * num) - (num2 * num2))) / (num2 * num2);
            double num24 = 1 + (num23 / 16384 * (4096 + (num23 * (-768 + (num23 * (320 - (175 * num23)))))));
            double num25 = num23 / 1024 * (256 + (num23 * (-128 + (num23 * (74 - (47 * num23))))));
            double num26 = num25 * y * (num22 + ((num25 / 4) * ((x * (-1 + (2 * num22 * num22))) - (num25 / 6 * num22 * (-3 + (4 * y * y)) * (-3 + (4 * num22 * num22))))));

            return num2 * num24 * (num18 - num26);
        }

        /// <summary>
        /// Vzdálenost [metry] mezi dvěma body.
        /// Odchylka max. 900 m.
        /// </summary>
        /// <remarks>
        /// Algoritmus: https://www.geodatasource.com/developers/sql
        /// </remarks>
        /// <param name="latitude1">Zeměpisná šířka v DegDec bodu 1.</param>
        /// <param name="longitude1">Zeměpisná délka v DegDec bodu 1.</param>
        /// <param name="latitude2">Zeměpisná šířka v DegDec bodu 2.</param>
        /// <param name="longitude2">Zeměpisná délka v DegDec bodu 2.</param>
        /// <returns>Vzdálenost v metrech.</returns>
        public static double GetDistanceByCircle(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            const double RADIUS = 6378137; //přesnost 900 m

            var lat1 = MyGeo.ToRAD(latitude1);
            var lon1 = MyGeo.ToRAD(longitude1);
            var lat2 = MyGeo.ToRAD(latitude2);
            var lon2 = MyGeo.ToRAD(longitude2);

            return RADIUS * Math.Acos((Math.Sin(lat1) * Math.Sin(lat2)) + (Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1)));
        }

        /// <summary>
        /// Vzdálenost [metry] od zadaného bodu.
        /// </summary>
        /// <param name="coordinate">Bod, ke kterému je počítána vzdálenost.</param>
        /// <returns>Vzdálenost v metrech.</returns>
        public double Distance(WGS84Coordinate coordinate)
        {
            return GetDistance(this.LatitudeDec, this.LongitudeDec, coordinate.LatitudeDec, coordinate.LongitudeDec);
        }

        /// <summary>
        /// Vzdálenost [metry] od zadaného bodu.
        /// </summary>
        /// <param name="coordinate">Bod, ke kterému je počítána vzdálenost.</param>
        /// <param name="latitude1">Zeměpisná šířka bodu, ke kterému je počítána vzdálenost.</param>
        /// <param name="longitude1">Zeměpisná délka bodu, ke kterému je počítána vzdálenost.</param>
        /// <returns>Vzdálenost v metrech.</returns>
        public double Distance(double latitude, double longitude)
        {
            return GetDistance(this.LatitudeDec, this.LongitudeDec, latitude, longitude);
        }

        /// <summary>
        /// Konstruktor pro zadání v systému DegDec.
        /// </summary>
        /// <param name="latitudeDec">Zeměpisná šířka v DegDec.</param>
        /// <param name="longitudeDec">Zeměpisná délka v DegDec.</param>
        public WGS84Coordinate(double latitudeDec, double longitudeDec)
        {
            LatitudeDec = latitudeDec;
            LongitudeDec = longitudeDec;
        }

        /// <summary>
        /// Konstruktor pro zadání v systému DegGeo.
        /// </summary>
        /// <param name="latitudeDegrees">Stupně zeměpisné šířky.</param>
        /// <param name="latitudeMinutes">Minuty zeměpisné šířky.</param>
        /// <param name="latitudeSeconds">Sekundy zeměpisné šířky.</param>
        /// <param name="longitudeDegrees">Stupně zeměpisné délky.</param>
        /// <param name="longitudeMinutes">Minuty zeměpisné délky.</param>
        /// <param name="longitudeSeconds">Sekundy zeměpisné délky.</param>
        public WGS84Coordinate(double latitudeDegrees, double latitudeMinutes, double latitudeSeconds, double longitudeDegrees, double longitudeMinutes, double longitudeSeconds)
        {
            LatitudeDec = CoordinateTransformation.DegreesToDec(latitudeDegrees, latitudeMinutes, latitudeSeconds);
            LongitudeDec = CoordinateTransformation.DegreesToDec(longitudeDegrees, longitudeMinutes, longitudeSeconds);
        }


        #region Methods

        /// <summary>
        /// Otevře prohlížeč s mapou pro aktuální souřadnici.
        /// </summary>
        /// <param name="mapServer">Mapový server [B]ing, [O]pen Street Map, [S]eznam, [G]oogle</param>
        public void OpenMap(string mapServer)
        {
            try
            {
                switch (mapServer.ToUpper())
                {
                    case "B": //Bing
                        const string FORMAT_B = @"https://bing.com/maps/default.aspx?sp=point.{0}_{1}";
                        Process.Start(string.Format(CultureInfo.InvariantCulture, FORMAT_B, LatitudeDec, LongitudeDec));
                        break;
                    case "O": //OSM (open street map)
                        const string FORMAT_O = @"https://www.openstreetmap.org/?mlat={0}&mlon={1}#map=16/{0}/{1}";
                        Process.Start(string.Format(CultureInfo.InvariantCulture, FORMAT_O, LatitudeDec, LongitudeDec));
                        break;
                    case "S": //Seznam
                        const string FORMAT_S = @"https://mapy.cz/zakladni?x={1}&y={0}&z=16&source=coor&id={1}%2C{0}";
                        Process.Start(string.Format(CultureInfo.InvariantCulture, FORMAT_S, LatitudeDec, LongitudeDec));
                        break;
                    default: //Google
                        const string FORMAT_G = @"https://maps.google.com/maps?q={0},{1}";
                        Process.Start(string.Format(CultureInfo.InvariantCulture, FORMAT_G, LatitudeDec, LongitudeDec));
                        break;
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Pokusí se ze zadaného řetězce vydolovat souřadnice WGS84.
        /// Pokud uspěje, vrací true a v out parametru souřadnici.
        /// </summary>
        /// <remarks>
        /// Podporuje následující formáty:
        ///  GoogleDec:  49.4593683, 18.3572658
        ///  SeznamDec:  49.4593683N, 18.3572658E
        ///  SeznamDeg1: N 49°27.56210', E 18°21.43595'
        ///  SeznamDeg2: 49°27'33.726"N, 18°21'26.157"E
        /// 
        ///  šířka může být: N,S
        ///  délka může být: E,W
        /// </remarks>
        /// <param name="value">Konverotvaný řetězec.</param>
        /// <param name="wgs84Coordinate">Nová souřadnice.</param>
        /// <returns></returns>
        public static bool TryParse(string value, out WGS84Coordinate wgs84Coordinate)
        {
            wgs84Coordinate = null;

            if (string.IsNullOrEmpty(value?.Trim()))
                return false;

            if (TryParseGoogleDec(value, out wgs84Coordinate))
                return true;

            if (TryParseSeznamDec(value, out wgs84Coordinate))
                return true;

            if (TryParseSeznamDegMin(value, out wgs84Coordinate))
                return true;

            if (TryParseSeznamDegMinSec(value, out wgs84Coordinate))
                return true;

            return false;
        }

        /// <summary>
        /// Konverze řetězce Googlu "49.4593683, 18.3572658"
        /// </summary>
        /// <param name="value"></param>
        /// <param name="wgs84Coordinate"></param>
        /// <returns></returns>
        public static bool TryParseGoogleDec(string value, out WGS84Coordinate wgs84Coordinate)
        {
            const RegexOptions REGX_OPTION = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace;
            const string REGX = @"(?<lat>[+|-]?\d{1,3}.\d+)\s*,\s*(?<lng>[+|-]?\d{1,3}.\d+)\s*";

            var match = new Regex(REGX, REGX_OPTION).Match(value);

            double lat;
            double lng;

            wgs84Coordinate = null;

            if (match.Success
                && double.TryParse(match.Groups["lat"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out lat)
                && double.TryParse(match.Groups["lng"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out lng)
                )

            {
                wgs84Coordinate = new WGS84Coordinate(lat, lng);
            }

            return wgs84Coordinate != null;
        }

        public static bool TryParseSeznamDec(string value, out WGS84Coordinate wgs84Coordinate)
        {
            const RegexOptions REGX_OPTION = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace;
            const string REGX = @"(?<lat>\d{1,3}.\d*)\s*(?<latSign>N|S)\s*,\s*(?<lng>\d{1,3}.\d*)\s*(?<lngSign>E|W)";

            var match = new Regex(REGX, REGX_OPTION).Match(value);

            double lat;
            double lng;

            wgs84Coordinate = null;

            if (match.Success
                && double.TryParse(match.Groups["lat"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out lat)
                && double.TryParse(match.Groups["lng"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out lng)
                )

            {
                if (match.Groups["latSign"].Value.Equals("S", StringComparison.OrdinalIgnoreCase))
                    lat = -lat;

                if (match.Groups["lngSign"].Value.Equals("W", StringComparison.OrdinalIgnoreCase))
                    lng = -lng;

                wgs84Coordinate = new WGS84Coordinate(lat, lng);
            }

            return wgs84Coordinate != null;
        }

        public static bool TryParseSeznamDegMin(string value, out WGS84Coordinate wgs84Coordinate)
        {
            const RegexOptions REGX_OPTION = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace;
            const string REGX = @"(?<latSign>[N|S])\s*(?<latDeg>\d{1,3})°(?<latMin>\d{1,2}.\d*)'\s*,\s*(?<lngSign>[E|W])\s*(?<lngDeg>\d{1,3})°(?<lngMin>\d{1,2}.\d*)'";

            var match = new Regex(REGX, REGX_OPTION).Match(value);

            double latDeg;
            double latMin;

            double lngDeg;
            double lngMin;

            wgs84Coordinate = null;

            if (match.Success
                && double.TryParse(match.Groups["latDeg"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out latDeg)
                && double.TryParse(match.Groups["latMin"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out latMin)
                && double.TryParse(match.Groups["lngDeg"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out lngDeg)
                && double.TryParse(match.Groups["lngMin"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out lngMin)
                )

            {
                var lat = latDeg + latMin / 60d;
                if (match.Groups["latSign"].Value.Equals("S", StringComparison.OrdinalIgnoreCase))
                    lat = -lat;

                var lng = lngDeg + lngMin / 60d;
                if (match.Groups["lngSign"].Value.Equals("W", StringComparison.OrdinalIgnoreCase))
                    lng = -lng;

                wgs84Coordinate = new WGS84Coordinate(lat, lng);
            }

            return wgs84Coordinate != null;
        }

        public static bool TryParseSeznamDegMinSec(string value, out WGS84Coordinate wgs84Coordinate)
        {
            const RegexOptions REGX_OPTION = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace;
            const string REGX = @"(?<latDeg>\d{1,3})°(?<latMin>\d{1,2})'(?<latSec>\d{1,2}.\d*)""\s*(?<latSign>N|S)\s*,\s* (?<lngDeg>\d{1,3})°(?<lngMin>\d{1,2})'(?<lngSec>\d{1,2}.\d*)""\s*(?<lngSign>E|W)";

            var match = new Regex(REGX, REGX_OPTION).Match(value);

            double latDeg;
            double latMin;
            double latSec;

            double lngDeg;
            double lngMin;
            double lngSec;

            wgs84Coordinate = null;

            if (match.Success
                && double.TryParse(match.Groups["latDeg"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out latDeg)
                && double.TryParse(match.Groups["latMin"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out latMin)
                && double.TryParse(match.Groups["latSec"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out latSec)
                && double.TryParse(match.Groups["lngDeg"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out lngDeg)
                && double.TryParse(match.Groups["lngMin"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out lngMin)
                && double.TryParse(match.Groups["lngSec"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out lngSec)
                )

            {
                var lat = latDeg + latMin / 60d + latSec / 3600d;
                if (match.Groups["latSign"].Value.Equals("S", StringComparison.OrdinalIgnoreCase))
                    lat = -lat;

                var lng = lngDeg + lngMin / 60d + lngSec / 3600d;
                if (match.Groups["lngSign"].Value.Equals("W", StringComparison.OrdinalIgnoreCase))
                    lng = -lng;

                wgs84Coordinate = new WGS84Coordinate(lat, lng);
            }

            return wgs84Coordinate != null;
        }

        #endregion //Methods

        #region Properties

        /// <summary>
        /// Zeměpisná šířka ve stupních (X).
        /// </summary>
        public double LatitudeDec
        {
            get => _LatitudeDec;
            set
            {
                _LatitudeDec = value;
                _LatitudeRad = _LatitudeDec * DEG_TO_RAD;
            }
        }

        /// <summary>
        /// Zeměpisná šířka v radiánech (X).
        /// </summary>
        public double LatitudeRad => _LatitudeRad;

        /// <summary>
        /// Zeměpisná šířka v geografických stupních.
        /// </summary>
        public int LatitudeDegrees => CoordinateTransformation.DecToDegrees(LatitudeDec);

        /// <summary>
        /// Zeměpisná šířka v geografických minutách.
        /// </summary>
        public int LatitudeMinutes => CoordinateTransformation.DecToMinutes(LatitudeDec);

        /// <summary>
        /// Zeměpisná šířka v geografických vteřinách.
        /// </summary>
        public double LatitudeSeconds => CoordinateTransformation.DecToSeconds(LatitudeDec);

        /// <summary>
        /// Zeměpisná délka ve stupních (Y).
        /// </summary>
        public double LongitudeDec
        {
            get => _LongitudeDec;
            set
            {
                _LongitudeDec = value;
                _LongitudeRad = _LongitudeDec * DEG_TO_RAD;
            }
        }

        /// <summary>
        /// Zeměpisná délka v radiánech (Y).
        /// </summary>
        public double LongitudeRad => _LongitudeRad;

        /// <summary>
        /// Zeměpisná délka v geografických stupních.
        /// </summary>
        public int LongitudeDegrees => CoordinateTransformation.DecToDegrees(LongitudeDec);

        /// <summary>
        /// Zeměpisná délka v geografických minutách.
        /// </summary>
        public int LongitudeMinutes => CoordinateTransformation.DecToMinutes(LongitudeDec);

        /// <summary>
        /// Zeměpisná délka v geografických vteřinách.
        /// </summary>
        public double LongitudeSeconds => CoordinateTransformation.DecToSeconds(LongitudeDec);

        /// <summary>
        /// Souřadnice v S42.
        /// </summary>
        public S42Coordinate S42Coordinate => Transformation.TransformS42(this);

        /// <summary>
        /// Souřadnice v JTSK EPSG:2065 (+).
        /// </summary>
        public JTSK2065Coordinate JTSK2065Coordinate => Transformation.TransformJTSK2065(this);

        /// <summary>
        /// Souřadnice v JTSK EPSG:5514 (-).
        /// </summary>
        public JTSK5514Coordinate JTSK5514Coordinate => Transformation.TransformJTSK5514(this);

        /// <summary>
        /// Souřadnice v EMEPGrid50x50.
        /// </summary>
        public EMEPGrid50x50Coordinate EMEPGrid50x50Coordinate => Transformation.TransformEMEPGrid50x50(this);

        /// <summary>
        /// Souřadnice v EMEPGrid01x01.
        /// </summary>
        public EMEPGrid01x01Coordinate EMEPGrid01x01Coordinate => Transformation.TransformEMEPGrid01x01(this);

        /// <summary>
        /// Souřadnice v UTM.
        /// </summary>
        public UTMCoordinate UTMCoordinate => Transformation.TransformUTM(this);

        /// <summary>
        /// Souřadnice v UTM33N.
        /// </summary>
        public UTMZoneCoordinate UTM33NCoordinate => Transformation.TransformUTMzone(this, 33);

        /// <summary>
        /// Zeměpisná souřadnice v geografickém zápisu.
        /// </summary>
        /// <returns></returns>
        public string ToDegString()
        {
            return ToDegString(System.Globalization.CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Zeměpisná souřadnice v geografickém zápisu.
        /// </summary>
        /// <returns></returns>
        public string ToDegString(IFormatProvider provider)
        {
            return $"{CoordinateTransformation.ToDegString(provider, LatitudeDec)} {CoordinateTransformation.ToDegString(provider, LongitudeDec)}";
        }

        /// <summary>
        /// Řetězcová reprezentace objektu.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ToString(System.Globalization.CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Řetězcová reprezentace objektu.
        /// </summary>
        /// <returns></returns>
        public string ToString(IFormatProvider provider)
        {
            return string.Format(provider, "{0}\u00b0 {1}\u00b0", LatitudeDec, LongitudeDec);
        }

        #endregion //Properties
    }

    #endregion //Souřadnicové systémy

}
