using Address2Map.BusinessController;
using Address2Map.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Address2Map.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AddressController : ControllerBase
    {

        private readonly ILogger<AddressController> _logger;
        private readonly AddressBusinessController addressBusinessController;
        public AddressController(ILogger<AddressController> logger, AddressBusinessController addressBusinessController)
        {
            _logger = logger;
            this.addressBusinessController = addressBusinessController;
        }
        /// <summary>
        /// Autocomplete city
        /// </summary>
        /// <param name="cityName"></param>
        /// <returns></returns>
        [HttpGet("AutocompleteCity/{cityName}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Model.City>))]
        [ProducesResponseType(400)]
        public ActionResult<IEnumerable<Model.City>> AutocompleteCity([FromRoute] string cityName)
        {
            try
            {
                return Ok(addressBusinessController.AutocompleteCity(cityName));
            }
            catch (Exception exc)
            {
                return BadRequest(new ProblemDetails() { Detail = exc.Message + (exc.InnerException != null ? $";\n{exc.InnerException.Message}" : "") + "\n" + exc.StackTrace, Title = exc.Message, Type = exc.GetType().ToString() });
            }

        }
        /// <summary>
        /// Autocomplete street for specific city
        /// </summary>
        /// <param name="cityCode"></param>
        /// <param name="streetName"></param>
        /// <returns></returns>
        [HttpGet("AutocompleteStreet/{cityCode}/{streetName}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Model.City>))]
        [ProducesResponseType(400)]
        public ActionResult<IEnumerable<Model.City>> AutocompleteStreet([FromRoute] uint cityCode, [FromRoute] string streetName)
        {
            try
            {
                return Ok(addressBusinessController.AutocompleteStreet(cityCode, streetName));
            }
            catch (Exception exc)
            {
                return BadRequest(new ProblemDetails() { Detail = exc.Message + (exc.InnerException != null ? $";\n{exc.InnerException.Message}" : "") + "\n" + exc.StackTrace, Title = exc.Message, Type = exc.GetType().ToString() });
            }

        }
    }
}