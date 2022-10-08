using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTSK_S42_WGS84_Krovak_GPS
{

    /// <summary>
    /// Geografické vymezení čtverců EMEP podle pokynů uvedených v dokumentu Guidelines: 
    /// http://www.ceip.at/fileadmin/inhalte/emep/2014_Guidelines/ece.eb.air.125_ADVANCE_VERSION_reporting_guidelines_2013.pdf  
    /// Relevantní pasáže(čl. 14, 28, 47-50)
    /// 
    /// Stručně: 
    /// 1. čtverce jsou definovány velikostí 0.1 x 0.1 geografického stupně souřadnic WGS
    /// 2. čtverec je definován souřadnicemi sváho středu
    /// 3. dle http://www.ceip.at/new_emep-grid se udávý jako lng-lat
    /// 4. ale zde je zase lan-lng http://www.unece.org/fileadmin/DAM/env/documents/2012/air/EMEP_36th/n_3_EMEP_note_on_grid_scale__projection_and_reporting.pdf
    /// 
    /// Příklad: 
    /// čtverec 18.15; 49.05
    /// má souřadnice [18.10; 19.00] - (18.20; 49.10)
    /// 
    /// Poznámka 2:
    /// EMEP - Evropský program monitorování a hodnocení (European Monitoring and Evaluation Program) 
    /// byl zřízen organizacemi UNECE (United Economic Commission for Europe), 
    /// WMO (World Meteorological Organization) a UNEP (United Nations Environment Programme) 
    /// v roce 1977.
    /// </summary>
    public class EMEPGrid01x01Coordinate
    {
        /// <summary>
        /// Velikost hrany gridu ve stupních
        /// </summary>
        public const double GRID_HALF_SIZE = 0.05; //velikost hrany gridu ve stupních

        /// <summary>
        /// Zeměpisná délka.
        /// </summary>
        public double LatitudeDec { get; }

        /// <summary>
        /// Zeměpisná šířka.
        /// </summary>
        public double LongitudeDec { get; }

        /// <summary>
        /// Pokud jsou zadané hodnoty středy EMEP čtverců, vrtací True.
        /// </summary>
        /// <remarks>
        /// Technicky se musíé jednat o násobky 0.1 zvětšené o 0.05
        /// </remarks>
        public bool IsValid => IsGridable(LatitudeDec) && IsGridable(LongitudeDec);

        /// <summary>
        /// Konstruktor. 
        /// </summary>
        /// <param name="latitudeDec">Zeměpisná délka.</param>
        /// <param name="longitudeDec">Zeměpisná šířka.</param>
        public EMEPGrid01x01Coordinate(double latitudeDec, double longitudeDec)
        {
            LatitudeDec = latitudeDec;
            LongitudeDec = longitudeDec;
        }

        /// <summary>
        /// Wgs souřadnice středu EMEP čtverce.
        /// </summary>
        /// <param name="wgs84Coordinate"></param>
        public EMEPGrid01x01Coordinate(WGS84Coordinate wgs84Coordinate)
            : this(wgs84Coordinate.LatitudeDec, wgs84Coordinate.LongitudeDec)
        { }

        /// <summary>
        /// Souřadnice levého horního rohu.
        /// </summary>
        public WGS84Coordinate LeftTopCorner => new WGS84Coordinate(LatitudeDec + GRID_HALF_SIZE, LongitudeDec - GRID_HALF_SIZE);

        /// <summary>
        /// Souřadnice pravého horního rohu.
        /// </summary>
        public WGS84Coordinate RightTopCorner => new WGS84Coordinate(LatitudeDec + GRID_HALF_SIZE, LongitudeDec + GRID_HALF_SIZE);

        /// <summary>
        /// Souřadnice levého dolního rohu.
        /// </summary>
        public WGS84Coordinate LeftBottomCorner => new WGS84Coordinate(LatitudeDec - GRID_HALF_SIZE, LongitudeDec - GRID_HALF_SIZE);

        /// <summary>
        /// Souřadnice pravého dolního rohu.
        /// </summary>
        public WGS84Coordinate RightBottomCorner => new WGS84Coordinate(LatitudeDec - GRID_HALF_SIZE, LongitudeDec + GRID_HALF_SIZE);

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
            return string.Format(provider, "EMEP 0.1\u00b0x0.1\u00b0: {0:0.00}; {1:0.00}{2}", LatitudeDec, LongitudeDec, ((IsValid) ? string.Empty : " (nevalidní)"));
        }

        internal static double Gridable(double value)
        {
            return (Math.Floor(value * 10) / 10) + 0.05;
        }

        /// <summary>
        /// Vrací True v případě, že číslo je GRIDABLE (tj. je o 0.05 větší násobku 0.10)
        /// </summary>
        /// <param name="value">Kontrolovaná hodnota.</param>
        /// <returns></returns>
        private static bool IsGridable(double value)
        {
            return Math.Abs(value - Gridable(value)) < 0.0000001;
        }
    }

}
