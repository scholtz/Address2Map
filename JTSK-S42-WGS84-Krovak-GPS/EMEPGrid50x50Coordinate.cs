using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTSK_S42_WGS84_Krovak_GPS
{


    /// <summary>
    /// Celoevropský program EMEP rozděluje území do sítě čtverců 50 x 50 km v doméně 132x159 bodů.
    /// Síť je založena na polární stereografické projekci v reálném prostoru.
    /// 
    /// Poznámka 1:
    /// Platnost tohoto gridu je dočasně do roku 2012, kdy má být nahrazen jiným systémem.
    /// 
    /// Poznámka 2:
    /// EMEP - Evropský program monitorování a hodnocení (European Monitoring and Evaluation Program) 
    /// byl zřízen organizacemi UNECE (United Economic Commission for Europe), 
    /// WMO (World Meteorological Organization) a UNEP (United Nations Environment Programme) 
    /// v roce 1977.
    /// 
    /// Další informace na http://www.emep.int
    /// </summary>
    public class EMEPGrid50x50Coordinate
    {
        /// <summary>
        /// X-ová vzdálenost gridu od severního pólu.
        /// </summary>
        public const int NorthPoleX = 8;

        /// <summary>
        /// Y-ová vzdálenost gridu od severního pólu.
        /// </summary>
        public const int NorthPoleY = 110;

        /// <summary>
        /// Vzdálenost gridu mezi severním pólem a rovníkem.
        /// </summary>
        public static double M
        {
            get
            {
                const double d = 50;                        // velikost gridu na 60 st. sš
                const double FI0 = Math.PI * 60d / 180d;    // definování šířky - 60 stupňů [RAD]
                const double R = 6370;                      // poloměr zeměkoule

                return R / d * (1 + Math.Sin(FI0));         // počet vzdáleností gridu mezi sev. polem a rovníkem
            }
        }

        public int X;
        public int Y;

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="x">Souřadnice X.</param>
        /// <param name="y">Souřadnice Y.</param>
        public EMEPGrid50x50Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Řetězcová reprezentace objektu.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"EMEP 50x50: {{{X}; {Y}}}";
        }
    }


}
