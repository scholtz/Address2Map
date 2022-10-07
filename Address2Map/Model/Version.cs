using System.Globalization;

namespace Address2Map.Model
{
    /// <summary>
    /// API version information
    /// </summary>
    public class Version
    {
        /// <summary>
        /// Instance identifier. Every application runtime has its own guid. If 3 pods are launched in kubernetes, it is possible to identify instance by this identifier
        /// </summary>
        public string InstanceIdentifier { get; set; }
        /// <summary>
        /// Last time when instance has been reset
        /// </summary>
        public string InstanceStartedAt { get; set; }
        /// <summary>
        /// Application name
        /// </summary>
        public string ApplicationName { get; set; }
        /// <summary>
        /// Full docker image with name and version
        /// For example crm5g-service-settings:20211006.8
        /// </summary>
        public string DockerImage { get; set; }
        /// <summary>
        /// Docker image version
        /// </summary>
        public string DockerImageVersion { get; set; }
        /// <summary>
        /// Build number from devops or github actions
        /// </summary>
        public string BuildNumber { get; set; }
        /// <summary>
        /// Application dll version
        /// </summary>
        public string DLLVersion { get; set; }
        /// <summary>
        /// Version of commit or changeset
        /// </summary>
        public string SourceVersion { get; set; }
        /// <summary>
        /// Dll build time
        /// </summary>
        public string BuildTime { get; set; }
        /// <summary>
        /// Culture info
        /// </summary>
        public string Culture { get; set; } = CultureInfo.CurrentCulture.Name;
        /// <summary>
        /// Status to validate functionality
        /// </summary>
        public string Status { get; set; }
    }
}
