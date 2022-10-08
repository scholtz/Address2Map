using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTSK_S42_WGS84_Krovak_GPS
{

    /// <summary>
    /// Souřadnicový systém S-42.
    /// </summary>
    /// <remarks>
    /// Souřadnicový systém S-42 používá Krasovského elipsoid s referenčním bodem v Pulkavu. 
    /// Souřadnice bodů jsou vyjádřené v 6° a 3° pásech Gaussova zobrazení. Geodetickým základem 
    /// je astronomicko-geodetická síť (AGS), která byla vyrovnána v mezinárodním spojení a do ní 
    /// byla transformovaná Jednotná trigonometrická síť katastrální (JTSK).
    /// </remarks>
    public class S42Coordinate
    {
        public double X;
        public double Y;

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="x">Souřadnice X.</param>
        /// <param name="y">Souřadnice Y.</param>
        public S42Coordinate(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Souřadnice ve WGS84.
        /// </summary>
        public WGS84Coordinate WGS84Coordinate => Transformation.TransformWGS84(this);

        /// <summary>
        /// Řetězcová reprezentace objektu.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"S42: {{{X}m; {Y}m}}";
        }
    }
}
