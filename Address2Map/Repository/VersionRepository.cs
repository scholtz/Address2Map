namespace Address2Map.Repository
{
    /// <summary>
    /// Version repository
    /// </summary>
    public class VersionRepository
    {
        /// <summary>
        /// show current version
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="start"></param>
        /// <param name="dllVersion"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public Model.Version GetVersion(string instanceId, DateTimeOffset start, string dllVersion, string status = "")
        {
            var ret = new Model.Version();
            var versionFileDocker = "docker-version.txt";
            if (System.IO.File.Exists(versionFileDocker))
            {
                ret.DockerImageVersion = System.IO.File.ReadAllText(versionFileDocker).Trim();
            }
            var versionFile = "version.json";
            if (System.IO.File.Exists(versionFile))
            {
                try
                {
                    var versionFileContent = System.IO.File.ReadAllText(versionFile);
                    ret = Newtonsoft.Json.JsonConvert.DeserializeObject<Model.Version>(versionFileContent);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                }
            }
            else if (System.IO.File.Exists(versionFile))
            {
                var version = System.IO.File.ReadAllText(versionFile).Trim();
                var versionData = version.Split('|');
                if (versionData.Length == 3)
                {
                    var pos = versionData[0].LastIndexOf('-');
                    if (pos > 0)
                    {
                        ret.ApplicationName = versionData[0].Substring(0, pos - 1).Trim();
                        ret.BuildNumber = versionData[0].Substring(pos + 1).Trim();
                    }
                    ret.DLLVersion = versionData[1].Trim();
                    ret.BuildTime = versionData[2].Trim();
                }
            }
            else
            {
                ret.DLLVersion = dllVersion;
            }
            if (string.IsNullOrEmpty(versionFile))
            {
                ret.DLLVersion = dllVersion;
            }
            ret.InstanceStartedAt = start.ToString("o");
            ret.InstanceIdentifier = instanceId;
            ret.Status = status;
            return ret;
        }
    }
}
