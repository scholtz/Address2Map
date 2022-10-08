using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTSK_S42_WGS84_Krovak_GPS
{

    /// <summary>
    /// Souřadnicový systém Jednotné trigonometrické sítě katastrální (EPSG 2065 S-JTSK/Krovak South-West).
    /// Tzv. kladné
    /// </summary>
    /// <remarks>
    /// Souřadnicový systém jednotné trigonometrické sítě katastrální (S-JTSK) je definován 
    /// Besselovým elipsoidem s referenčním bodem Hermannskogel, Křovákovým zobrazením 
    /// (dvojité konformní kuželové zobrazení v obecné poloze), převzatými prvky sítě vojenské 
    /// triangulace (orientací, rozměrem i polohou na elipsoidu) a jednotnou trigonometrickou 
    /// sítí katastrální. Křovákovo zobrazení je jednotné pro celý stát. Navrhl a propracoval 
    /// jej Ing. Josef Křovák roku 1922.
    /// 
    /// Vztah mezi souřadnicemi „záporného“ X ,Y a „kladného“ x,y Křováka (tedy mezi EPSG 5514 a EPSG 2065) je tento: X = -y a Y = -x.
    /// 
    /// Platí, že pro ČR je X > Y.
    /// Zobrazují se v pořadí Y, X
    /// Používají je geodeti pro svá měření v terénu.
    /// Zdroj: http://geoportal.cuzk.cz/(S(un51vr0e1spynjiydli1bnri))/Default.aspx?mode=TextMeta&amp;text=about_FAQ&amp;side=about&amp;menu=6
    /// </remarks>
    public class JTSK2065Coordinate
    {
        public double X { get; }
        public double Y { get; }

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="x">Souřadnice X.</param>
        /// <param name="y">Souřadnice Y.</param>
        public JTSK2065Coordinate(double x, double y)
        {
            /* kontrola dočasně vypnuta 2017-11-30 
            if (x <= y)
                throw new ArgumentOutOfRangeException($"Hodnota x musí být větší jak y. (x={x}; y={y})");

            if (x < 0)
                throw new ArgumentOutOfRangeException($"Hodnota x musí být kladná. (x={x})");

            if (y < 0)
                throw new ArgumentOutOfRangeException($"Hodnota y musí být kladná. (y={y})");
            */

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
            return $"JTSK2065: {{{Y}m; {X}m}}";
        }
    }


}
