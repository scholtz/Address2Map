using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTSK_S42_WGS84_Krovak_GPS
{
    #region Transformation

    /// <summary>
    /// Transformuje souřadnice ze systému ITRF (v podstatě WGS84) na S42.
    /// </summary>
    /// <remarks>
    /// Výpočet byl oproti vzoru dosti brutálně optimalizován, tj. není tak názorný
    /// a množství různých volání funkcí bylo zjednodušeno až na úroveň konstanty.
    /// </remarks>
    public class Transformation
    {
        private const double DEG_TO_RAD = Math.PI / 180d; //0.0174532925199432958   rad = deg * DEG_TO_RAD
        private const double RAD_TO_DEG = 1.0 / DEG_TO_RAD; //565.48667764616278292   deg = rad / DEG_TO_RAD

        /// <summary>
        /// Transformuje souřadnice ze systému ITRF (v podstatě WGS84) na S42.
        /// </summary>
        /// <param name="wgs84Coordinate">Transformované souřadnice.</param>
        /// <returns></returns>
        public static S42Coordinate TransformS42(WGS84Coordinate wgs84Coordinate)
        {
            double o3 = 6378245d / Math.Sqrt(1 - 0.0066934216229659511d * Math.Pow(Math.Sin(wgs84Coordinate.LatitudeRad), 2)); // N
            double p3 = Math.Pow(Math.Tan(wgs84Coordinate.LatitudeRad), 2); // T
            double q3 = 0.0067385254146834989d * Math.Pow(Math.Cos(wgs84Coordinate.LatitudeRad), 2);// C
            double r3 = (wgs84Coordinate.LongitudeRad - 0.26179938779914941d) * Math.Cos(wgs84Coordinate.LatitudeRad);// A
            double s3 =
                  6367558.4970123032d * wgs84Coordinate.LatitudeRad
                - 16036.479939776922d * Math.Sin(2d * wgs84Coordinate.LatitudeRad)
                + 16.827654579200246d * Math.Sin(4d * wgs84Coordinate.LatitudeRad)
                - 0.02179177355292761d * Math.Sin(6d * wgs84Coordinate.LatitudeRad)
                ; // M

            return new S42Coordinate(
                  3500123.2862402d + o3 * (r3 + (1d - p3 + q3) * Math.Pow(r3, 3) / 6d + (5d - 18d * p3 + Math.Pow(p3, 2) + 72d * q3 - 0.57277466024809742d) * Math.Pow(r3, 5) / 120d)
                , 42.93530495d + (s3 + o3 * Math.Tan(wgs84Coordinate.LatitudeRad) * (Math.Pow(r3, 2) / 2d + (5d - p3 + 9d * q3 + 4d * Math.Pow(q3, 2)) * Math.Pow(r3, 4) / 24d + (58.776286613154447d - 58d * p3 + Math.Pow(r3, 2) + 600d * q3) * Math.Pow(r3, 6) / 720d))
                );
        }

        /// <summary>
        /// Transformuje souřadnice ze systému ITRF (v podstatě WGS84) na JTSK ESP:2065.
        /// Převod používá Hrdninův algoritmus 
        /// </summary>
        /// <remarks>
        /// Výpočet byl oproti vzoru dosti brutálně optimalizován, tj. není tak názorný
        /// a množství různých volání funkcí bylo zjednodušeno až na úroveň konstanty.
        /// </remarks>
        /// <param name="wgs84Coordinate">Transformované souřadnice.</param>
        public static JTSK2065Coordinate TransformJTSK2065(WGS84Coordinate wgs84Coordinate)
        {
            //--- Výpočet pravoúhlých souřadnic z geodetických souřadnic.
            double ro = 6378137d / Math.Sqrt(1d - 0.00669437999014133d * Math.Pow(Math.Sin(wgs84Coordinate.LatitudeRad), 2));

            double x1 = ro * Math.Cos(wgs84Coordinate.LatitudeRad) * Math.Cos(wgs84Coordinate.LongitudeRad);
            double y1 = ro * Math.Cos(wgs84Coordinate.LatitudeRad) * Math.Sin(wgs84Coordinate.LongitudeRad);
            double z1 = ro * Math.Sin(wgs84Coordinate.LatitudeRad) * 0.99330562000985867d;
            //--- 

            //--- transformace pravoúhlých souřadnic
            double x2 = -570.69d + 0.999996457d * (0.0000255065325768538d * y1 + x1 - 0.0000076928295663736721d * z1);
            double y2 = -85.69d + 0.999996457d * (-0.0000255065325768538d * x1 + y1 + 0.00002423200589058494d * z1);
            double z2 = -462.84d + 0.999996457d * (0.0000076928295663736721d * x1 + z1 - 0.00002423200589058494d * y1);
            //---

            //--- Výpočet geodetických souřadnic z pravoúhlých souřadnic.
            double p = Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2));
            double theta = Math.Atan(z2 * 1.0033539847919968d / p);
            double B = Math.Atan((z2 + 42707.884210431082d * Math.Pow(Math.Sin(theta), 3)) / (p - 42565.121440450319d * Math.Pow(Math.Cos(theta), 3)));
            double L = 2 * Math.Atan(y2 / (p + x2));
            //---

            //--- finální výpočet
            const double e = 0.081696831215303d;
            const double n = 0.97992470462083d;
            const double sinVQ = 0.420215144586493d;
            const double cosVQ = 0.907424504992097d;
            const double alfa = 1.000597498371542d;

            double sinB = Math.Sin(B);
            double t = 1.00685001861538d * Math.Exp(alfa * Math.Log(Math.Pow(1d + sinB, 2) / (1 - Math.Pow(sinB, 2)) * Math.Exp(e * Math.Log((1d - e * sinB) / (1d + e * sinB)))));
            double sinU = (t - 1d) / (t + 1d);
            double cosU = Math.Sqrt(1 - Math.Pow(sinU, 2));
            double V = alfa * L;
            double sinS = 0.863499969506341d * sinU + 0.504348889819882d * cosU * (cosVQ * Math.Cos(V) + sinVQ * Math.Sin(V));
            double cosS = Math.Sqrt(1 - Math.Pow(sinS, 2));
            double sinD = (sinVQ * Math.Cos(V) - cosVQ * Math.Sin(V)) * cosU / cosS;
            double D = Math.Atan(sinD / Math.Sqrt(1 - Math.Pow(sinD, 2)));
            double ro2 = 12310230.12797036d * Math.Exp(-n * Math.Log((1d + sinS) / cosS));
            //---

            return new JTSK2065Coordinate(ro2 * Math.Cos(n * D), ro2 * Math.Sin(n * D));
        }

        /// <summary>
        /// Transformuje souřadnice ze systému ITRF (v podstatě WGS84) na JTSK ESP:5514.
        /// Převod používá Hrdninův algoritmus 
        /// </summary>
        /// <remarks>
        /// Výpočet byl oproti vzoru dosti brutálně optimalizován, tj. není tak názorný
        /// a množství různých volání funkcí bylo zjednodušeno až na úroveň konstanty.
        /// </remarks>
        /// <param name="wgs84Coordinate">Transformované souřadnice.</param>
        public static JTSK5514Coordinate TransformJTSK5514(WGS84Coordinate wgs84Coordinate)
        {
            return TransformJTSK5514(TransformJTSK2065(wgs84Coordinate));
        }

        /// <summary>
        /// Transformuje souřadnice ze systému ITRF (v podstatě WGS84) na EMEPGrid50x50.
        /// Převod použivá oficiální algoritmus.
        /// </summary>
        /// <param name="wgs84Coordinate">Transformované souřadnice.</param>
        /// <returns></returns>
        public static EMEPGrid50x50Coordinate TransformEMEPGrid50x50(WGS84Coordinate wgs84Coordinate)
        {
            double FI = Math.PI * wgs84Coordinate.LatitudeDec / 180d;
            double Lambda = Math.PI * wgs84Coordinate.LongitudeDec / 180d;

            const double LAMBDA0 = Math.PI * (-32d) / 180d;     // úhel rotace -32 stupnu, tj. délky rovnoběžné s osou Y [RAD]

            double indexX = EMEPGrid50x50Coordinate.NorthPoleX + EMEPGrid50x50Coordinate.M * Math.Tan(Math.PI / 4d - FI / 2d) * Math.Sin(Lambda - LAMBDA0);
            double indexY = EMEPGrid50x50Coordinate.NorthPoleY - EMEPGrid50x50Coordinate.M * Math.Tan(Math.PI / 4d - FI / 2d) * Math.Cos(Lambda - LAMBDA0);

            return new EMEPGrid50x50Coordinate(Convert.ToInt32(indexX), Convert.ToInt32(indexY));
        }

        /// <summary>
        /// Transformuje souřadnice ze systému ITRF (v podstatě WGS84) na EMEPGrid 0.1 x 0.1 st.
        /// Velikost buňky je zhruba 7 x 18 km (délka, šířka).
        /// </summary>
        /// <param name="wgs84Coordinate">Transformované souřadnice.</param>
        /// <returns></returns>
        public static EMEPGrid01x01Coordinate TransformEMEPGrid01x01(WGS84Coordinate wgs84Coordinate)
        {
            return new EMEPGrid01x01Coordinate(EMEPGrid01x01Coordinate.Gridable(wgs84Coordinate.LatitudeDec), EMEPGrid01x01Coordinate.Gridable(wgs84Coordinate.LongitudeDec));
        }

        /// <summary>
        /// Transformuje souřadnice ze systému ITRF (v podstatě WGS84) na EMEPGrid 0.1 x 0.1 st.
        /// Velikost buňky je zhruba 7 x 18 km (délka, šířka).
        /// </summary>
        /// <param name="latitudeDec">Zeměpisná šířka transformované souřadnice.</param>
        /// <param name="longitudeDec">Zeměpisná délka transformované souřadnice.</param>
        /// <returns></returns>
        public static EMEPGrid01x01Coordinate TransformEMEPGrid01x01(double latitudeDec, double longitudeDec)
        {
            return new EMEPGrid01x01Coordinate(new WGS84Coordinate(latitudeDec, longitudeDec));
        }

        /// <summary>
        /// Transformuje souřadnice ze systému EMEPGrid50x50 (v podstatě WGS84) na ITRF (v podstatě WGS84).
        /// Převod použivá oficiální algoritmus.
        /// </summary>
        /// <param name="emepGrid50x50Coordinate">Transformované souřadnice.</param>
        /// <returns></returns>
        public static WGS84Coordinate TransformWGS84(EMEPGrid50x50Coordinate emepGrid50x50Coordinate)
        {
            double X = emepGrid50x50Coordinate.X;
            double Y = emepGrid50x50Coordinate.Y;

            const double Lambda0 = -32;  // úhel rotace -32 stupnu, tj. délky rovnoběžné s osou Y

            double r = Math.Sqrt(Math.Pow((X - EMEPGrid50x50Coordinate.NorthPoleX), 2) + Math.Pow((Y - EMEPGrid50x50Coordinate.NorthPoleY), 2));
            double FI = 90d - 360d / Math.PI * Math.Atan(r / EMEPGrid50x50Coordinate.M);
            double Lambda = Lambda0 + 180d / Math.PI * Math.Atan((X - EMEPGrid50x50Coordinate.NorthPoleX) / (EMEPGrid50x50Coordinate.NorthPoleY - Y));

            return new WGS84Coordinate(FI, Lambda);
        }

        /// <summary>
        /// Transformuje souřadnice JTSK5514Coordinate na JTSK2065Coordinate.
        /// </summary>
        /// <param name="jtskCoordinate">JTSK souřadnice EPSG 5514.</param>
        /// <returns></returns>
        public static JTSK2065Coordinate TransformJTSK2065(JTSK5514Coordinate jtskCoordinate)
        {
            return new JTSK2065Coordinate(-jtskCoordinate.Y, -jtskCoordinate.X);
        }

        /// <summary>
        /// Transformuje souřadnice JTSK2065Coordinate na JTSK5514Coordinate.
        /// </summary>
        /// <param name="jtskCoordinate">JTSK souřadnice EPSG 2065.</param>
        /// <returns></returns>
        public static JTSK5514Coordinate TransformJTSK5514(JTSK2065Coordinate jtskCoordinate)
        {
            return new JTSK5514Coordinate(-jtskCoordinate.Y, -jtskCoordinate.X);
        }

        /// <summary>
        /// Transformuje souřadnice ze systému JTSK EPSG:2065 na ITRF (v podstatě WGS84).
        /// </summary>
        /// <remarks>
        /// Výpočet byl oproti vzoru dosti brutálně optimalizován, tj. není tak názorný
        /// a množství různých volání funkcí bylo zjednodušeno až na úroveň konstanty.
        /// 
        /// Zdrojový kód: http://www.alena.ilcik.cz/gps/souradnice/JTSKtoWGS.htm
        /// používá evidentně Hrdinův algoritmus:
        ///   Hrdina, Z.: Prepocet z S-JTSK do WGS-84. 2002.
        ///   http://gpsweb.cz/JTSK-WGS.htm.
        /// </remarks>
        /// <param name="jtsk2065Coordinate">Transformované souřadnice.</param>
        /// <returns></returns>
        public static WGS84Coordinate TransformWGS84(JTSK2065Coordinate jtsk2065Coordinate)
        {
            double H = 45;

            //--- Výpočet zeměpisných souřadnic z rovinných souřadnic
            const double e = 0.081696831215303;
            const double sinVQ = 0.420215144586493;
            const double cosVQ = 0.907424504992097;

            double ro1 = Math.Sqrt(Math.Pow(jtsk2065Coordinate.X, 2) + Math.Pow(jtsk2065Coordinate.Y, 2));
            double D = 2d * Math.Atan(jtsk2065Coordinate.Y / (ro1 + jtsk2065Coordinate.X)) / 0.97992470462083d;
            double S = 2d * Math.Atan(Math.Exp(1.0204865693093612d * Math.Log(12310230.12797036d / ro1))) - Math.PI / 2d;
            double sinU = 0.863499969506341d * Math.Sin(S) - 0.504348889819882d * Math.Cos(S) * Math.Cos(D);
            double cosU = Math.Sqrt(1d - Math.Pow(sinU, 2));
            double sinDV = Math.Sin(D) * Math.Cos(S) / cosU;
            double cosDV = Math.Sqrt(1d - Math.Pow(sinDV, 2));
            double Ljtsk = 2d * Math.Atan((sinVQ * cosDV - cosVQ * sinDV) / (1d + cosVQ * cosDV + sinVQ * sinDV)) / 1.000597498371542d;
            double t = Math.Exp(1.9988057168391598d * Math.Log((1d + sinU) / cosU / 1.003419163966575d));
            double pom = (t - 1d) / (t + 1d);

            double sinB;

            do
            {
                sinB = pom;
                pom = t * Math.Exp(e * Math.Log((1 + e * sinB) / (1 - e * sinB)));
                pom = (pom - 1) / (pom + 1);
            }
            while (Math.Abs(pom - sinB) > 1e-15);

            double Bjtsk = Math.Atan(pom / Math.Sqrt(1 - Math.Pow(pom, 2)));
            //---

            //--- Pravoúhlé souřadnice ve S-JTSK
            double ro2 = 6377397.15508d / Math.Sqrt(1 - 0.0066743722306217279d * Math.Pow(Math.Sin(Bjtsk), 2));
            double x = (ro2 + H) * Math.Cos(Bjtsk) * Math.Cos(Ljtsk);
            double y = (ro2 + H) * Math.Cos(Bjtsk) * Math.Sin(Ljtsk);
            double z = (0.99332562776937827d * ro2 + H) * Math.Sin(Bjtsk);
            //---

            //--- Pravoúhlé souřadnice v WGS-84
            const double wx = -0.00002423200589058494d;
            const double wy = -0.0000076928295663736721d;
            const double wz = -0.0000255065325768538d;

            double xn = 570.69d + 1.000003543d * (x + wz * y - wy * z);
            double yn = 85.69d + 1.000003543d * (-wz * x + y + wx * z);
            double zn = 462.84d + 1.000003543d * (wy * x - wx * y + z);
            //---

            //--- Geodetické souřadnice v systému WGS-84
            double p = Math.Sqrt(Math.Pow(xn, 2) + Math.Pow(yn, 2));
            double theta = Math.Atan(zn * 1.0033640898209764d / p);
            double B = Math.Atan((zn + 42841.31151331366d * Math.Pow(Math.Sin(theta), 3)) / (p - 42697.672707180056d * Math.Pow(Math.Cos(theta), 3)));
            double L = 2 * Math.Atan(yn / (p + xn));
            //---

            return new WGS84Coordinate(B / Math.PI * 180, L / Math.PI * 180);
        }

        /// <summary>
        /// Transformuje souřadnice ze systému JTSK EPSG:5514 na ITRF (v podstatě WGS84).
        /// </summary>
        /// <remarks>
        /// Detaily viz. TransformWGS84 pro TransformJTSK2065().
        /// </remarks>
        /// <param name="jtsk5514Coordinate">Transformované souřadnice.</param>
        /// <returns></returns>
        public static WGS84Coordinate TransformWGS84(JTSK5514Coordinate jtsk5514Coordinate)
        {
            return TransformWGS84(TransformJTSK2065(jtsk5514Coordinate));
        }

        /// <summary>
        /// Transformuje souřadnice ze systému S42 na ITRF (v podstatě WGS84).
        /// </summary>
        /// <remarks>
        /// Compiled by Gábor Timár, Eötvös University of Budapest, e-mail: timar@ludens.elte.hu
        /// References: František Kuska (1960): Matematická Kartografia. Slovenské Vydateľstvo Technickej Literatúry, Bratislava, 388 p.
        /// John P. Snyder (1987): Map projections - a working manual. USGS Prof. Paper 1395: 1-261
        /// Equations derived by József Varga (Technical University of Budapest) and Gábor Virág (FÖMI Space Geodesy Observatory, Penc, Hungary)
        /// were used for computing the Krovák projection.
        /// </remarks>
        /// <param name="s42Coordinate">Transformované souřadnice.</param>
        /// <returns></returns>
        public static WGS84Coordinate TransformWGS84(S42Coordinate s42Coordinate)
        {
            // MAXIMÁLNÍ PŘESNOST
            //  LAT (-0,0041415; 0,0050661) m
            //  LON (-0,0055585; 0,0086234) m
            double R2 = s42Coordinate.Y / 6367558.4970123032d
                + 0.0025184647775237596d * Math.Sin(s42Coordinate.Y / 3183779.2485061516)
                + 0.0000036998858962068768d * Math.Sin(s42Coordinate.Y / 1591889.6242530758d)
                + 0.0000000074446047831951984d * Math.Sin(s42Coordinate.Y / 1061259.7495020505d)
                + 0.000000000017026207045302084d * Math.Sin(s42Coordinate.Y / 795944.8121265379);

            // DOSTAČUJÍCÍ PŘESNOST
            // LAT (0,0000000; 0,0444900) m
            // LON (-0,0068361; 0,0109344) m
            //double R2 = s42Coordinate.Y / 6367558.4970123032d
            //    + 0.0025184647775237596d * Math.Sin(s42Coordinate.Y / 3183779.2485061516)
            //    + 0.0000036998858962069d * Math.Sin(s42Coordinate.Y / 1591889.6242530758d);

            double C1 = 0.0067385254146834989d * Math.Pow(Math.Cos(R2), 2);
            double T1 = Math.Pow(Math.Tan(R2), 2);
            double N1 = 6378245d / Math.Sqrt(1 - 0.0066934216229659511d * Math.Pow(Math.Sin(R2), 2));
            double D = (s42Coordinate.X - (500000 + Math.Truncate(s42Coordinate.X / 1000000) * 1000000)) / N1;
            double Fl1Rad = R2 - N1 * Math.Tan(R2) / 6335552.7170004258d * Math.Pow(1 - 0.0066934216229659511d * Math.Pow(Math.Sin(R2), 2), 1.5)
                * (0.5d * Math.Pow(D, 2) - (4.9393532712678487 + 3 * T1 + 10 * C1 - 4 * Math.Pow(C1, 2))
                    * Math.Pow(D, 4) / 24 + (61 + 90 * T1 + 298 * C1 + 45 * T1 * T1
                    - 1.6981084045002417d - 3 * Math.Pow(C1, 2)) * Math.Pow(D, 6) / 720
                  );
            double LaRad = (0.36651914291880922d + 0.10471975511965977 * (Math.Truncate(s42Coordinate.X / 1000000) - 4))
                + (D - (1 + 2 * T1 + C1) * Math.Pow(D, 3) / 6 + (5 - 2 * C1 + 28 * T1 - 3 * Math.Pow(C1, 2) + 0.053908203317467991d + 24 * Math.Pow(T1, 2)) * Math.Pow(D, 5) / 120)
                / Math.Cos(R2);

            double dFIsec = (-26 * Math.Sin(Fl1Rad) * Math.Cos(LaRad) + 121 * Math.Sin(Fl1Rad) * Math.Sin(LaRad) - 78 * Math.Cos(Fl1Rad) + 2.7045797937100424d * Math.Sin(2 * Fl1Rad))
                / 110576.25484489677d * Math.Pow((1 - 0.0066934216229659329d * Math.Pow(Math.Sin(Fl1Rad), 2)), 1.5);
            double dLAsec = (-26 * Math.Sin(LaRad) - 121 * Math.Cos(LaRad)) / 111321.37574842962d * Math.Sqrt(1 - 0.0066934216229659329d * Math.Pow(Math.Sin(Fl1Rad), 2))
                / Math.Cos(Fl1Rad);

            return new WGS84Coordinate(Fl1Rad * 57.295779513082323d + dFIsec, LaRad * 57.295779513082323d + dLAsec);
        }


        /// <summary>
        /// Transformuje souřadnice WGS84 na UTM
        /// </summary>
        /// <param name="wgs84Coordinate"></param>
        /// <returns></returns>
        public static UTMCoordinate TransformUTM(WGS84Coordinate wgs84Coordinate)
        {
            return TransformUTM(wgs84Coordinate, UTMCoordinate.GetUtmZoneNumber(wgs84Coordinate));
        }

        /// <summary>
        /// Transformuje souřadnice WGS84 na UTM tak, že vnutí číslo zóny bez ohledu na
        /// zeměpisnou šířku. Metoda je použita k přepočtu na UTM33N.
        /// </summary>
        /// <remarks>
        /// Online kalkulátopr a zobrazovač:
        /// https://www.latlong.net/lat-long-utm.html
        /// </remarks>
        /// <param name="wgs84Coordinate"></param>
        /// <param name="zoneNumber">Číslo zóny.</param>
        /// <returns></returns>
        private static UTMCoordinate TransformUTM(WGS84Coordinate wgs84Coordinate, int zoneNumber)
        {
            var elipsoid = Elipsoid.GetDefault();
            var eccSquared = elipsoid.EccSquared;
            var a = elipsoid.A;

            var latRad = wgs84Coordinate.LatitudeRad;
            var longRad = wgs84Coordinate.LongitudeRad;

            var longOrigin = (zoneNumber - 1) * 6 - 180 + 3;  //+3 puts origin in middle of zone
            var longOriginRad = longOrigin * DEG_TO_RAD;

            var utmZone = UTMCoordinate.GetUtmZoneLetter(wgs84Coordinate.LatitudeDec);

            var eccPrimeSquared = eccSquared / (1 - eccSquared);

            var N = a / Math.Sqrt(1 - eccSquared * Math.Sin(latRad) * Math.Sin(latRad));
            var T = Math.Tan(latRad) * Math.Tan(latRad);
            var C = eccPrimeSquared * Math.Cos(latRad) * Math.Cos(latRad);
            var A = Math.Cos(latRad) * (longRad - longOriginRad);

            var M = a * ((1 - eccSquared / 4 - 3 * eccSquared * eccSquared / 64 - 5 * eccSquared * eccSquared * eccSquared / 256) * latRad
                    - (3 * eccSquared / 8 + 3 * eccSquared * eccSquared / 32 + 45 * eccSquared * eccSquared * eccSquared / 1024) * Math.Sin(2 * latRad)
                    + (15 * eccSquared * eccSquared / 256 + 45 * eccSquared * eccSquared * eccSquared / 1024) * Math.Sin(4 * latRad)
                    - (35 * eccSquared * eccSquared * eccSquared / 3072) * Math.Sin(6 * latRad));

            var utmEasting = 0.9996 * N * (A + (1 - T + C) * A * A * A / 6
                    + (5 - 18 * T + T * T + 72 * C - 58 * eccPrimeSquared) * A * A * A * A * A / 120)
                    + 500000.0;

            var utmNorthing = 0.9996 * (M + N * Math.Tan(latRad) * (A * A / 2 + (5 - T + 9 * C + 4 * C * C) * A * A * A * A / 24
                    + (61 - 58 * T + T * T + 600 * C - 330 * eccPrimeSquared) * A * A * A * A * A * A / 720));

            if (wgs84Coordinate.LatitudeDec < 0)
                utmNorthing += 10000000.0;

            return new UTMCoordinate { Easting = utmEasting, Northing = utmNorthing, ZoneNumber = zoneNumber, ZoneLetter = utmZone };
        }

        /// <summary>
        /// Transformuje souřadnice WGS84 na UTMzone
        /// </summary>
        /// <param name="wgs84Coordinate"></param>
        /// <param name="zoneNumber">Číslo 'base zóny'.</param>
        /// <returns></returns>
        public static UTMZoneCoordinate TransformUTMzone(WGS84Coordinate wgs84Coordinate, int zoneNumber)
        {
            var tempCoordinate = TransformUTM(wgs84Coordinate, zoneNumber);

            return new UTMZoneCoordinate(tempCoordinate.Easting, tempCoordinate.Northing, zoneNumber);
        }

    }

    #endregion //Transformation
}
