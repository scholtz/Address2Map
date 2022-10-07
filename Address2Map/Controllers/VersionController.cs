using Address2Map.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Address2Map.Controllers
{
    /// <summary>
    /// Public version controller
    /// </summary>
    public class VersionController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly VersionRepository versionRepository;
        /// <summary>
        /// Constructor
        /// </summary>
        public VersionController(IConfiguration configuration, VersionRepository versionRepository)
        {
            this.configuration = configuration;
            this.versionRepository = versionRepository;
        }

        /// <summary>
        /// Returns version of the current api
        /// 
        /// For development purposes it returns version of assembly, for production purposes it returns string build by pipeline which contains project information, pipeline build version, assembly version, and build date
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200, Type = typeof(Model.Version))]
        [ProducesResponseType(400)]
        [HttpGet("Version")]
        public ActionResult<Model.Version> Get()
        {
            try
            {
                var ret = versionRepository?.GetVersion(
                    Startup.InstanceId,
                    Startup.Started,
                    GetType()?.Assembly?.GetName()?.Version?.ToString()
                );
                return Ok(ret);
            }
            catch (Exception exc)
            {
                return BadRequest(new ProblemDetails() { Detail = exc.Message + (exc.InnerException != null ? $";\n{exc.InnerException.Message}" : "") + "\n" + exc.StackTrace, Title = exc.Message, Type = exc.GetType().ToString() });
            }
        }
    }
}
