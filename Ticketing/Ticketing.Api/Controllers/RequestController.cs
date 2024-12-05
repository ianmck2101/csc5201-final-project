using Fetch.Api.Logic;
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

        [Route("NewRequest")]
        [HttpPost]
        public IActionResult CreateNewRequest()
        {
            _requestService.CreateNewRequest();

            return Ok();
        }
    }
}
