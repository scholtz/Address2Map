using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTSK_S42_WGS84_Krovak_GPS
{

    /// <summary>
    /// Seznam souřadnic WGS84.
    /// </summary>
    public class WGS84CoordinateList : List<WGS84Coordinate>
    {

        #region Helpers

        private string GetPointsAsString()
        {
            const string format = @"|{0},{1}";
            string result = string.Empty;

            foreach (WGS84Coordinate wgs84 in this)
            {
                result += string.Format(System.Globalization.CultureInfo.InvariantCulture, format, wgs84.LatitudeDec, wgs84.LongitudeDec);
            }

            return result;
        }

        #endregion //Helpers

        /// <summary>
        /// Otevře prohlížeč s mapou a zobrazí seznam bodů pro zadané souřadnice.
        /// </summary>
        public void OpenMapAsPoints()
        {
            var commandFormat = @"http://maps.google.com/maps/api/staticmap?size=640x640{1}&sensor=false&markers=color:yellow{0}";
            var itemFormat = @"|{0},{1}";
            var coordinates = string.Empty;
            var zoom = (this.Count > 1) ? string.Empty : "&zoom=15";

            foreach (var wgs84 in this)
            {
                coordinates += string.Format(System.Globalization.CultureInfo.InvariantCulture, itemFormat, wgs84.LatitudeDec, wgs84.LongitudeDec);
            }

            try
            {
                var command = string.Format(commandFormat, coordinates, zoom);
                System.Diagnostics.Process.Start(command);
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Otevře prohlížeč s mapou a zobrazí seznam bodů jako trasu.
        /// </summary>
        /// <remarks>
        /// http://maps.google.com/maps/api/staticmap?size=640x640&path=color:0xff0000FF|weight:10|50.699308,13.970686|50.515775,14.046808|50.319946,13.545316|50.360336,13.785165&sensor=false
        /// </remarks>
        public void OpenMapAsTrace()
        {
            var commandFormat = @"http://maps.google.com/maps/api/staticmap?size=640x640&sensor=false&path=color:0x0000ff90|weight:3{0}&markers=color:yellow|size:small{0}";
            var itemFormat = @"|{0},{1}";
            var coordinates = string.Empty;

            foreach (WGS84Coordinate wgs84 in this)
            {
                coordinates += string.Format(System.Globalization.CultureInfo.InvariantCulture, itemFormat, wgs84.LatitudeDec, wgs84.LongitudeDec);
            }

            try
            {
                var command = string.Format(commandFormat, coordinates);
                System.Diagnostics.Process.Start(command);
            }
            catch
            {
                // ignored
            }
        }
    }

}
