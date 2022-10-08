using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTSK_S42_WGS84_Krovak_GPS
{
    /// <summary>
    /// Knihovna geometrických funkcí.
    /// 
    /// Original creator https://raw.githubusercontent.com/xPaRi/ShowInMap/master/MyGeo.cs
    /// </summary>
    public class MyGeo
    {
        /// <summary>
        /// Označuje kvadrant.
        /// </summary>
        /// <remarks>
        /// Jedná se o určení počátku souřadnic [0, 0].
        /// </remarks>
        public enum Quadrant
        {
            /// <summary>
            /// Kvadrant nelze určit (šířka nebo výška jsou nulové).
            /// </summary>
            Unknown,

            /// <summary>
            /// První kvadrant, počátek souřadnic je vlevo dole.
            /// </summary>
            Quadrant1,

            /// <summary>
            /// Druhý kvadrant, počátek souřadnic je vpravo dole.
            /// </summary>
            Quadrant2,

            /// <summary>
            /// Třetí kvadrant, počátek souřadnic je vpravo nahoře.
            /// </summary>
            Quadrant3,

            /// <summary>
            /// Čtvrtý kvadrant, počátek souřadnic je vlevo nahoře.
            /// </summary>
            Quadrant4
        }

        /// <summary>
        /// Velikost jednoho radiánu ve stupních (RAD - DEG).
        /// </summary>
        public const double OneRAD = 180d / Math.PI;

        private MyGeo()
        { }

        /// <summary>
        /// Převede úhel ve stupních do radiánů.
        /// </summary>
        /// <param name="angleInDEG"></param>
        /// <returns></returns>
	    public static double ToRAD(double angleInDEG)
        {
            return angleInDEG / OneRAD;
        }

        /// <summary>
        /// Převede úhel v radiánech do stupňů.
        /// </summary>
        /// <param name="angleInRAD"></param>
        /// <returns></returns>
        public static double ToDEG(double angleInRAD)
        {
            return angleInRAD * OneRAD;
        }

        /// <summary>
        /// Určí kvadrant zadané plochy (bounds je dle Geo).
        /// </summary>
        /// <param name="bounds">Plocha.</param>
        /// <remarks>Pozor! Win kreslí v Q4.</remarks>
        /// <returns>Kvadrant.</returns>
        public static Quadrant GetQuadrant(RectangleF bounds)
        {
            int xSign = Math.Sign(bounds.Width);
            int ySign = Math.Sign(bounds.Height);

            if (xSign == 1 && ySign == 1)
                return Quadrant.Quadrant1;

            if (xSign == -1 && ySign == 1)
                return Quadrant.Quadrant2;

            if (xSign == -1 && ySign == -1)
                return Quadrant.Quadrant3;

            if (xSign == 1 && ySign == -1)
                return Quadrant.Quadrant4;

            return Quadrant.Unknown;

        }

        /// <summary>
        /// Určí kvadrant zadané plochy.
        /// </summary>
        /// <param name="bounds">Plocha.</param>
        /// <returns>Kvadrant.</returns>
        public static Quadrant GetQuadrant(Rectangle bounds)
        {
            return GetQuadrant(new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height));
        }

        /// <summary>
        /// Úhel mezi dvěma body.
        /// </summary>
        /// <remarks>
        /// Úhel, který svírá určitý směr (směr k pozorovanému objektu, ...)
        /// od směru východního. Úhel je orientovaný, zaleží tedy na směru měření úhlu.
        /// Měří se proti směru pohybu hodinových ručiček, tj. od východu k severu. 
        /// Měří se ve stupních.
        /// </remarks>
        /// <param name="basePoint">Výchozí bod (bod ze kterého se díváme do cíle).</param>
        /// <param name="targetPoint">Cílový bod (bod na který se díváme z cíle).</param>
        /// <returns>Úhel.</returns>
        public static double GetAngle(PointF basePoint, PointF targetPoint)
        {
            float xDelta = targetPoint.X - basePoint.X;
            float yDelta = targetPoint.Y - basePoint.Y;

            if (xDelta == 0) //protože v Atan se pak dělí nulou
                return 180 - 90 * Math.Sign(yDelta); //90 nebo 180
            else if (xDelta < 0) //2. a 3. kvadrant
                return 180 + Math.Atan(yDelta / xDelta) * OneRAD;
            else if (yDelta < 0) //4.kvadrant
                return 360 + Math.Atan(yDelta / xDelta) * OneRAD;
            else //1.kvadrant
                return Math.Atan(yDelta / xDelta) * OneRAD;
        }

        /// <summary>
        /// Azimut mezi dvěma body.
        /// </summary>
        /// <remarks>
        /// <seealso cref="Angle2Azimuth"/>
        /// </remarks>
        /// <param name="basePoint">Výchozí bod (bod ze kterého se díváme do cíle).</param>
        /// <param name="targetPoint">Cílový bod (bod na který se díváme z cíle).</param>
        /// <returns>Azimut.</returns>
        public static double GetAzimuth(PointF basePoint, PointF targetPoint)
        {
            return Angle2Azimuth(GetAngle(basePoint, targetPoint));
        }

        /// <summary>
        /// Převede geometrický úhel na azimut.
        /// </summary>
        /// <remarks>
        /// Azimut je orientovaný úhel, který svírá určitý směr (směr k pozorovanému objektu, ...)
        /// od směru severního. Úhel je orientovaný, zaleží tedy na směru měření úhlu.
        /// Měří se po směru pohybu hodinových ručiček, tj. od severu k východu. 
        /// Měří se ve stupních.
        /// Z definice vyplývá, že sever má azimut 0°, východ 90°, jih 180° a západ 270°.
        /// </remarks>
        /// <param name="angle">Úhel.</param>
        /// <returns>Azimut.</returns>
        public static double Angle2Azimuth(double angle)
        {
            if (angle > 90)
                return 450 - angle;
            else //if (angle<=90)
                return 90 - angle;
        }

        /// <summary>
        /// Dopočítá podle zadaných parametrů další body do úseku.
        /// </summary>
        /// <remarks>
        /// Do úseku mezi zadanými body (...Point) vloží v zadané vzdálenosti (step)
        /// další body, které vrátí v zadaném poli.
        /// Vzdálenost posledního bodu je ponechána tak, jak vyjde.
        /// Hraniční body (...Point) nejsou zahrnuty.
        /// </remarks>
        /// <param name="basePoint">Výchozí bod (bod ze kterého se díváme do cíle).</param>
        /// <param name="targetPoint">Cílový bod (bod na který se díváme z cíle).</param>
        /// <param name="step">Krok.</param>
        /// <returns>Pole bodů.</returns>
        public static PointF[] GetPoints(PointF basePoint, PointF targetPoint, double step)
        {
            if (step == 0)
                return null;
            else
            {
                double angle = GetAngle(basePoint, targetPoint);
                double distanceBT = Distance(basePoint, targetPoint);
                int count = (int)(distanceBT / step) - 1;
                PointF[] result = null;

                if (count > 0)
                    result = new PointF[count];

                for (int index = 1; index <= count; index++)
                    result[index - 1] = GetPoint(basePoint, angle, step * index);

                return result;
            }
        }

        /// <summary>
        /// Dopočítá podle zadaných parametrů další body do úseku.
        /// Body jsou však posunuty o step/2 směrem k basePoint.
        /// </summary>
        /// <remarks>
        /// Do úseku mezi zadanými body (...Point) vloží v zadané vzdálenosti (step)
        /// další body, které vrátí v zadaném poli.
        /// Vzdálenost posledního bodu je ponechána tak, jak vyjde.
        /// Hraniční body (...Point) nejsou zahrnuty.
        /// Používá se např. k výpočtu plochy nad výškopisem.
        /// </remarks>
        /// <param name="basePoint">Výchozí bod (bod ze kterého se díváme do cíle).</param>
        /// <param name="targetPoint">Cílový bod (bod na který se díváme z cíle).</param>
        /// <param name="step">Krok.</param>
        /// <returns>Pole bodů.</returns>
        public static PointF[] GetPointsForIntegral(PointF basePoint, PointF targetPoint, double step)
        {
            if (step == 0)
                return null;
            else
            {
                double angle = GetAngle(basePoint, targetPoint); //úhel mezi krajními body
                double distanceBT = Distance(basePoint, targetPoint); //vzdálenost mezi krajními body
                int count = (int)(distanceBT / step); //počet bodů

                PointF[] result = new PointF[count];

                for (int index = 1; index <= count; index++)
                {
                    result[index - 1] = GetPoint(basePoint, angle, step * (index - 0.5));
                }

                return result;
            }
        }

        /// <summary>
        /// Vrací bod vzdálený od výchozího bodu o zadanou vzdálenost pod zadaným úhlem.
        /// </summary>
        /// <param name="basePoint">Výchozí bod (bod ze kterého se díváme do cíle).</param>
        /// <param name="angle">Úhel.</param>
        /// <param name="distance">Vzdálenost bodu od 'basePoint'.</param>
        /// <returns>Bod.</returns>
        public static PointF GetPoint(PointF basePoint, double angle, double distance)
        {
            return new PointF(
                Convert.ToSingle(Math.Round(basePoint.X + Math.Cos(angle / OneRAD) * distance, 5))
                , Convert.ToSingle(Math.Round(basePoint.Y + Math.Sin(angle / OneRAD) * distance, 5))
                );
        }

        /// <summary>
        /// Vypočítá vzdálenost mezi dvěma zadanými body.
        /// </summary>
        /// <param name="basePoint">Výchozí bod (bod ze kterého se díváme do cíle).</param>
        /// <param name="targetPoint">Cílový bod (bod na který se díváme z cíle).</param>
        /// <returns>Vzdálenost mezi zadanými body.</returns>
        public static double Distance(PointF basePoint, PointF targetPoint)
        {
            float xDelta = targetPoint.X - basePoint.X;
            float yDelta = targetPoint.Y - basePoint.Y;

            return Math.Sqrt(Math.Pow(xDelta, 2) + Math.Pow(yDelta, 2));
        }

        /// <summary>
        /// Lineární interpolace.
        /// </summary>
        /// <remarks>
        /// Lineárně interpoluje hodnoty mezi 'startValue' a 'endValue'
        /// do pole o počtu prvků 'count'.
        /// Pole obsahuje 'startValue' s indexem 0 a 'endValue'
        /// s indexem 'count'-1.
        /// </remarks>
        /// <param name="startValue">Počáteční hodnota interpolace.</param>
        /// <param name="endValue">Koncová hodnota interpolace.</param>
        /// <param name="count">Počet prvků pole interpolovaných hodnot.</param>
        /// <returns>Pole interpolovaných hodnot.</returns>
        public static double[] LinearInterpolation(double startValue, double endValue, int count)
        {
            double delta = endValue - startValue; //rozdíl hodnot
            double k = delta / (count - 1); //koeficient lineární interpolace
            double[] result = new double[count]; //pole s výsledky

            for (int index = 0; index < count; index++)
                result[index] = startValue + k * index;

            return result;
        }


        /// <summary>
        /// Lineárně interpolovaná hodnota mezi dvěma body.
        /// </summary>
        /// <param name="x1">X-ová souřadnice výchozího bodu.</param>
        /// <param name="y1">Y-ová souřadnice výchozího bodu.</param>
        /// <param name="x2">X-ová souřadnice koncového bodu.</param>
        /// <param name="y2">Y-ová souřadnice koncového bodu.</param>
        /// <param name="x">X-ová souřadnice bodu pro který zjišťujeme Y.</param>
        /// <returns>Y.</returns>
        public static double LinearInterpolationValue(double x1, double y1, double x2, double y2, double x)
        {
            return y1 + ((y2 - y1) * (x - x1)) / (x2 - x1);
        }

        /// <summary>
        /// Vrací střed úseku zadaného koncovými body.
        /// </summary>
        /// <param name="basePoint">Výchozí bod.</param>
        /// <param name="targetPoint">Koncový bod.</param>
        /// <returns>Střed úseku.</returns>
        public static PointF GetCenterPoint(PointF basePoint, PointF targetPoint)
        {
            double distance = Distance(basePoint, targetPoint);
            double angle = GetAngle(basePoint, targetPoint);

            return GetPoint(basePoint, angle, distance / 2f);
        }

        /// <summary>
        /// Vytvoří pole bodů vyplňující zadanou plochu.
        /// </summary>
        /// <param name="startPoint">Počáteční souřadnice.</param>
        /// <param name="countX">Počet kroků v ose X.</param>
        /// <param name="countY">Počet kroků v ose Y.</param>
        /// <param name="stepX">Krok v ose X.</param>
        /// <param name="stepY">Krok v ose Y.</param>
        /// <returns>Pole bodů.</returns>
        public static PointF[] GeneratePoints(PointF startPoint, int countX, int countY, double stepX, double stepY)
        {
            //--- příprava
            countX = Convert.ToInt32(Math.Abs(countX));
            countY = Convert.ToInt32(Math.Abs(countY));

            PointF[] result = new PointF[countX * countY];
            int index = 0;
            //---

            for (int indexY = 0; indexY < countY; indexY++)
                for (int indexX = 0; indexX < countX; indexX++)
                    result[index++] = new PointF(
                        Convert.ToSingle(startPoint.X + (indexX * stepX))
                        , Convert.ToSingle(startPoint.Y + (indexY * stepY))
                        );

            return result;
        }

        /// <summary>
        /// Vytvoří pole bodů vyplňující zadanou plochu.
        /// </summary>
        /// <param name="startPoint">Počáteční souřadnice.</param>
        /// <param name="endPoint">Koncová souřadnice.</param>
        /// <param name="countX">Počet bodů v ose X.</param>
        /// <param name="countY">Počet bodů v ose Y.</param>
        /// <returns>Pole bodů.</returns>
        public static PointF[] GeneratePoints(PointF startPoint, PointF endPoint, int countX, int countY)
        {
            //--- příprava
            double stepX = (endPoint.X - startPoint.X) / (Math.Abs(countX) - 1);
            double stepY = (endPoint.Y - startPoint.Y) / (Math.Abs(countY) - 1);
            //---

            return GeneratePoints(startPoint, countX, countY, stepX, stepY);
        }

        /// <summary>
        /// Vytvoří pole bodů vyplňující zadanou plochu.
        /// </summary>
        /// <param name="startPoint">Počáteční souřadnice.</param>
        /// <param name="endPoint">Koncová souřadnice.</param>
        /// <param name="deltaX">Krok v ose X (použije se jeho absolutní hodnota).</param>
        /// <param name="deltaY">Krok v ose Y (použije se jeho absolutní hodnota).</param>
        /// <returns>Pole bodů.</returns>
        public static PointF[] GeneratePoints(PointF startPoint, PointF endPoint, double deltaX, double deltaY)
        {
            //--- příprava
            List<PointF> points = new List<PointF>();

            double stepX = (startPoint.X <= endPoint.X) ? Math.Abs(deltaX) : -Math.Abs(deltaX);
            double stepY = (startPoint.Y <= endPoint.Y) ? Math.Abs(deltaY) : -Math.Abs(deltaY);
            //---

            for (double y = startPoint.Y; (stepY > 0 && y < endPoint.Y) || (stepY < 0 && y > endPoint.Y); y += stepY)
                for (double x = startPoint.X; (stepX > 0 && x < endPoint.X) || (stepX < 0 && x > endPoint.X); x += stepX)
                    points.Add(new PointF(Convert.ToSingle(x), Convert.ToSingle(y)));

            PointF[] result = new PointF[points.Count];

            points.CopyTo(result);

            return result;
        }

        /// <summary>
        /// Vrací normalizovaný obdélník.
        /// </summary>
        /// <remarks>
        /// Normalizovaný obdélník má všechny rozměry kladné (Width i Height).
        /// </remarks>
        /// <param name="rectangle">Obdélník.</param>
        /// <returns>Normalizovaný obdélník.</returns>
        public static RectangleF GetNormalizeRectangle(RectangleF rectangle)
        {
            float x1 = (rectangle.Width >= 0) ? rectangle.Left : rectangle.Right;
            float x2 = (rectangle.Width >= 0) ? rectangle.Right : rectangle.Left;

            float y1 = (rectangle.Height >= 0) ? rectangle.Top : rectangle.Bottom;
            float y2 = (rectangle.Height >= 0) ? rectangle.Bottom : rectangle.Top;

            return RectangleF.FromLTRB(x1, y1, x2, y2);
        }

        /// <summary>
        /// Vrací normalizovaný obdélník.
        /// </summary>
        /// <remarks>
        /// Normalizovaný obdélník má všechny rozměry kladné (Width i Height).
        /// </remarks>
        /// <param name="rectangle">Obdélník.</param>
        /// <returns>Normalizovaný obdélník.</returns>
        public static Rectangle GetNormalizeRectangle(Rectangle rectangle)
        {
            int x1 = (rectangle.Width >= 0) ? rectangle.Left : rectangle.Right;
            int x2 = (rectangle.Width >= 0) ? rectangle.Right : rectangle.Left;

            int y1 = (rectangle.Height >= 0) ? rectangle.Top : rectangle.Bottom;
            int y2 = (rectangle.Height >= 0) ? rectangle.Bottom : rectangle.Top;

            return Rectangle.FromLTRB(x1, y1, x2, y2);
        }
    }

}
