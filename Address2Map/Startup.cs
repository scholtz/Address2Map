namespace Address2Map
{

    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Identifies specific run of the application
        /// </summary>
        public readonly static string InstanceId = Guid.NewGuid().ToString();

        /// <summary>
        /// Identifies specific run of the application
        /// </summary>
        public readonly static DateTimeOffset Started = DateTimeOffset.Now;
    }
}
