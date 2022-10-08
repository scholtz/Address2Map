using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTSK_S42_WGS84_Krovak_GPS
{

    #region AngleDeg

    /// <summary>
    /// Úhel ve stupních, minutách a vteřinách.
    /// </summary>
    public struct AngleDeg
    {
        #region Fields

        internal static string FormatString = "{0}\u00b0 {1}' {2:0.0000}\"";

        private int _Degrees; //Stupně.
        private int _Minutes; //Minuty
        private double _Seconds; //Vteřiny

        #endregion //Fields

        #region Constructors

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="degrees">Stupně.</param>
        /// <param name="minutes">Minuty.</param>
        /// <param name="seconds">Vteřiny.</param>
        public AngleDeg(int degrees, int minutes, double seconds)
        {
            _Degrees = degrees;
            _Minutes = minutes;
            _Seconds = seconds;
        }

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="angleDec">Úhel v desetinném vyjádření.</param>
        public AngleDeg(double angleDec)
        {

            double degrees = Math.Floor(angleDec);
            double minutes = Math.Round((angleDec - degrees) * 60d, 10); //zaokrouhlení (10) je klíčové
            double seconds = Math.Round((minutes - Math.Floor(minutes)) * 60d, 8); //zaokrouhlení (8) je klíčové

            minutes = Math.Floor(minutes);

            _Degrees = Convert.ToInt32(degrees);
            _Minutes = Convert.ToInt32(minutes);
            _Seconds = seconds;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Stupně.
        /// </summary>
        public int Degrees
        {
            get
            {
                return _Degrees;
            }
            set
            {
                _Degrees = value;
            }
        }

        /// <summary>
        /// Minuty.
        /// </summary>
        public int Minutes
        {
            get
            {
                return _Minutes;
            }
            set
            {
                _Minutes = value;
            }
        }

        /// <summary>
        /// Vteřiny.
        /// </summary>
        public double Seconds
        {
            get
            {
                return _Seconds;
            }
            set
            {
                _Seconds = value;
            }
        }

        #endregion //Properties

        #region Public

        /// <summary>
        /// Úhel v desetinném vyjádření.
        /// </summary>
        /// <returns></returns>
        public double ToDec()
        {
            return Degrees + ((Minutes * 60d + Seconds) / 3600d);
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
            return string.Format(provider, FormatString, Degrees, Minutes, Seconds);
        }

        /// <summary>
        /// Operace porovnání rovnosti.
        /// </summary>
        /// <param name="obj1">První porovnávaný objekt.</param>
        /// <param name="obj2">Druhý porovnávaný objekt.</param>
        /// <returns>
        /// True - u identických objektů (i pokud jsou oba Null)
        /// , False - u nestejných objektů
        /// </returns>
        public static bool operator ==(AngleDeg obj1, AngleDeg obj2)
        {
            if ((object)obj1 != null)
                return (obj1.Equals(obj2));
            if ((object)obj2 != null)
                return (obj2.Equals(obj1));
            return (true);
        }


        /// <summary>
        /// Operace porovnání nerovnosti.
        /// </summary>
        /// <remarks>
        /// <returns>
        /// True - u nestejných objektů
        /// False - u identických objektů
        /// False - pokud jsou oba objekty null
        /// </returns>
        public static bool operator !=(AngleDeg obj1, AngleDeg obj2)
        {
            if ((object)obj1 != null)
                return (!obj1.Equals(obj2));
            if ((object)obj2 != null)
                return (!obj2.Equals(obj1));
            return (false);
        }

        /// <summary>
        /// Porovnání objektů téže třídy.
        /// </summary>
        /// <param name="obj">Objekt porovnávaný s aktuálním objektem.</param>
        /// <returns>
        /// True - u identických objektů
        /// False - u nestejných objektů
        /// </returns>
        public override bool Equals(object obj)
        {
            return
                Degrees.Equals(((AngleDeg)obj).Degrees)
                && Minutes.Equals(((AngleDeg)obj).Minutes)
                && Seconds.Equals(((AngleDeg)obj).Seconds)
                ;
        }

        public override int GetHashCode()
        {
            return Convert.ToInt32(ToDec());
        }

        #endregion //Public
    }

    #endregion //AngleDeg

}
