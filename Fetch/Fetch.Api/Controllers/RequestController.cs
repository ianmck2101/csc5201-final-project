using Fetch.Api.Logic;
using Fetch.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace Fetch.Api.Controllers
{
    [ApiController]
    [Route("Request")]
    public class RequestController : Controller
    {
        private readonly IRequestService _requestService;

        public RequestController(IRequestService requestService)
        {
            _requestService = requestService ?? throw new ArgumentNullException(nameof(requestService));
        }

        [Route("New")]
        [HttpPost]
        public IActionResult CreateNewRequest([FromBody] NewRequest newRequest)
        {
            if(newRequest == null || newRequest.Request == null)
            {
                return BadRequest();
            }

            _requestService.CreateNewRequest(newRequest.Request);
            return Created();
        }

        [Route("")]
        [HttpGet]
        public IActionResult GetAllRequests()
        {
            var requests = _requestService.GetAllRequests();
            return Json(requests);
        }

        [Route("{id}")]
        [HttpGet]
        public IActionResult GetRequest([FromRoute] int id)
        {
            var result = _requestService.GetRequest(id);

            if (result == null)
            {
                return NotFound($"Request with ID {id} was not found.");
            }

            return Json(result);
        }

        [Route("{id}/delete")]
        [HttpDelete]
        public IActionResult DeleteRequest([FromRoute] int id)
        {
            var result = _requestService.DeleteRequest(id);

            if (!result)
            {
                return NotFound($"Request with ID {id} was not found.");
            }

            return Ok($"Request with ID {id} has been deleted.");
        }
    }
}
