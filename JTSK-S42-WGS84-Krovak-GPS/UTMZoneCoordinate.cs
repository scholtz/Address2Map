using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTSK_S42_WGS84_Krovak_GPS
{

    /// <summary>
    /// Souřadnice s fixní zónou. 
    /// </summary>
    /// <remarks>
    /// Požívá se například pro přepočet WGS84 na UTM33N    '
    /// Řeší to, že ČR je na dvou dlaždicích UTM33N a UTM34N.
    /// Teď se to bude tvářit jako by byla na 33N
    /// </remarks>
    public class UTMZoneCoordinate
    {
        public double X { get; }
        public double Y { get; }
        public int ZoneNumber { get; }

        public UTMZoneCoordinate(double x, double y, int zoneNumber)
        {
            X = x;
            Y = y;
            ZoneNumber = zoneNumber;
        }

        public override string ToString()
        {
            return $"{X}; {Y}; zone: {ZoneNumber}";
        }
    }
}
