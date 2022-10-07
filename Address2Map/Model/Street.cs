namespace Address2Map.Model
{
    /// <summary>
    /// Street
    /// </summary>
    public class Street
    {
        /// <summary>
        /// Street code from rujan
        /// </summary>
        public uint Code { get; set; }
        /// <summary>
        /// Street name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Clear text
        /// </summary>
        public string Slug { get; set; }

        public override bool Equals(object? obj)
        {
            return Code.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        public override string? ToString()
        {
            return $"Street:{Code}:{Name}";
        }
    }
}
