using System.Security.Claims;
using Fetch.Api.Logic;
using Fetch.Models.Data;
using Fetch.Models.Request;
using Fetch.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fetch.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("Request")]
    public class RequestController : Controller
    {
        private readonly IRequestService _requestService;
        private readonly IUserService _userService;

        public RequestController(IRequestService requestService, IUserService userService)
        {
            _requestService = requestService ?? throw new ArgumentNullException(nameof(requestService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [Route("New")]
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [Authorize(Roles = "Requestor")]
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
        [ProducesResponseType(200, Type = typeof(LoadAllRequestsResponse))]
        [Authorize(Roles = "Requestor,Provider")]
        public IActionResult GetAllRequests()
        {
            var requests = _requestService.GetAllRequests();
            return Json(new LoadAllRequestsResponse() { Requests = requests});
        }

        [Route("{id}")]
        [HttpGet]
        [ProducesResponseType(200, Type= typeof(LoadSingleRequestResponse))]
        [ProducesResponseType(404)]
        [Authorize(Roles = "Requestor,Provider")]
        public IActionResult GetRequest([FromRoute] int id)
        {
            var result = _requestService.GetRequest(id);

            if (result == null)
            {
                return NotFound($"Request with ID {id} was not found.");
            }

            return Json(new LoadSingleRequestResponse() { Request = result});
        }

        [Route("{id}/delete")]
        [HttpDelete]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "Requestor")]
        public IActionResult DeleteRequest([FromRoute] int id)
        {
            var result = _requestService.DeleteRequest(id);

            if (!result)
            {
                return NotFound($"Request with ID {id} was not found.");
            }

            return Ok($"Request with ID {id} has been deleted.");
        }

        [Route("{id}/commands/accept/{providerId}")]
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "Provider")]
        public IActionResult AcceptRequest([FromRoute] int id, [FromRoute] int providerId)
        {
            var result = _requestService.AcceptRequest(id, providerId);

            if(!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [Route("{id}/commands/cancel")]
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "Requestor")]
        public IActionResult CancelRequest([FromRoute] int id)
        {
            var result = _requestService.CancelRequest(id);

            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [Route("providerView")]
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [Authorize(Roles = "Provider")]
        public IActionResult LoadProviderView()
        {
            // Extract the username from the token claims
            var username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("Invalid token: No username found");
            }

            // Fetch provider details based on the username
            var provider = _userService.GetProviderByUsername(username);
            if (provider == null)
            {
                return NotFound("Provider not found for this user");
            }

            // Query all request associations for the provider
            var associations = _requestService.GetAssociationsByProviderId(provider);

            return Ok(associations);
        }
    }
}
