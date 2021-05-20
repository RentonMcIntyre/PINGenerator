using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PinGenerator.Service.Services;

namespace PinGenerator.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PinGeneratorController : ControllerBase
    {
        private readonly ILogger<PinGeneratorController> _logger;
        private readonly IPinGeneratorService pinGeneratorService;

        public PinGeneratorController(ILogger<PinGeneratorController> logger, IPinGeneratorService pinGeneratorService)
        {
            _logger = logger;
            this.pinGeneratorService = pinGeneratorService;
        }

        /// <summary>
        /// Tests if PINs have been initialized and initializes them if not.
        /// </summary>
        /// <returns>A boolean value indicating whether initialisation has succeeded.</returns>
        [HttpGet("/pin/initialize")]
        public async Task<IActionResult> InitializePins()
        {
            var response = await pinGeneratorService.InitializePins();

            return Ok(response);
        }

        /// <summary>
        /// Queries the DataStore for the specified number of randomly selected PINs and ensures allocation is reset when necessary.
        /// </summary>
        /// <param name="requested">The number of PINs requested by the user.</param>
        /// <returns>A list of PIN objects equal in length to the requested number.</returns>
        [HttpGet("/pin/get-pins/{requested}")]
        public async Task<IActionResult> GetPINs(int requested)
        {
            var response = await pinGeneratorService.GetPINs(requested);

            return Ok(response);
        }
    }
}
