namespace Address2Map.Model
{
    /// <summary>
    /// Street number specification
    /// </summary>
    public class StreetNumberRule
    {
        public const uint MinFrom = 0;
        public const uint MaxTo = 10000;

        /// <summary>
        /// Street number - from
        /// </summary>
        public uint From { get; set; } = MinFrom;
        /// <summary>
        /// Street number - to
        /// </summary>
        public uint To { get; set; } = MaxTo;
        /// <summary>
        /// Street number series type
        /// </summary>
        public StreetNumberSeriesType SeriesType { get; set; }

        public override bool Equals(object? obj)
        {
            var rule = obj as StreetNumberRule;

            if (rule == null)
            {
                return false;
            }

            return From == rule.From
                && To == rule.To
                && SeriesType == rule.SeriesType;
        }

        private static StreetNumberRule _emptyRule;
        public static StreetNumberRule EmptyRule()
        {
            if (_emptyRule == null)
            {
                _emptyRule = new StreetNumberRule { From = MinFrom, To = MaxTo, SeriesType = StreetNumberSeriesType.All };
            }
            return _emptyRule;
        }
    }
}
