using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Http;
using PKHeX.API.Services;

namespace PKHeX.API.Controllers
{

    public class DocsController : ControllerBase
	{

        private readonly IEnumerable<EndpointDataSource> _endpointSources;

        private JSONService JSONService { get; }

        public DocsController(IEnumerable<EndpointDataSource> endpointSources, JSONService jsonService)
        {
            _endpointSources = endpointSources;
            JSONService = jsonService;

        }

        /// <summary>
        /// Returns API documentation
        /// </summary>
        /// <returns>API information</returns>
		/// <response code="200">Returns API wiki link</response>
		/// <response code="400">Request invalid</response>    
        [HttpGet("docs")]
        public async Task<OkObjectResult> Docs()
        {
            var message = "PKHeX API documentation can be found here: https://github.com/Project-PokeBots/PKHeX-API/wiki";

            var JsonResponse = await JSONService.ProcessJsonMessage(Response.StatusCode, message);

            return Ok(JsonResponse);
        }


        /// <summary>
        /// Returns all supported endpoints
        /// </summary>
        /// <returns>Returns all supported API endpoints</returns>
        /// <response code="200">Returns list of supported endpoints</response>
        /// <response code="400">Request invalid</response>    
        [HttpGet("endpoints")]
        public async Task<ActionResult> Endpoints()
        {
            var endpoints = _endpointSources
                .SelectMany(es => es.Endpoints)
                .OfType<RouteEndpoint>();
            var output = endpoints.Select(
                e =>
                {
                    var controller = e.Metadata
                        .OfType<ControllerActionDescriptor>()
                        .FirstOrDefault();
                    var action = controller != null
                        ? $"{controller.ControllerName}.{controller.ActionName}"
                        : null;
                    var controllerMethod = controller != null
                        ? $"{controller.ControllerTypeInfo.FullName}:{controller.MethodInfo.Name}"
                        : null;
                    return new
                    {
                        Method = e.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()?.HttpMethods?[0],
                        Route = $"/{e.RoutePattern.RawText.TrimStart('/')}",
                        Action = action,
                        ControllerMethod = controllerMethod
                    };
                }
            );

            return Ok(output);
        }

        /// <summary>
        /// Returns API supported games
        /// </summary>
        /// <returns>List of supported games</returns>
		/// <response code="200">Returns list of supported game</response>
		/// <response code="400">Request invalid</response>     
        [HttpGet("games")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<OkObjectResult> Games()
        {
            var message = "Gen1: RBY, Gen2: GSC, Gen3: RS, Gen4: PT, Gen5: B2W2, Gen6: ORAS, Gen7: USUM, Gen7b: LGPE, Gen8: SWSH, Gen8b: BDSP, Gen8a: PLA, Gen9: SV";

            var JsonResponse = await JSONService.ProcessJsonMessage(Response.StatusCode, message);

            return Ok(JsonResponse);
        }
    }
}