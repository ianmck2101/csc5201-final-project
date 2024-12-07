using Fetch.Models.Data;

namespace Fetch.Models.Response
{
    public class LoadAllRequestsResponse
    {
        public IEnumerable<BaseRequest> Requests { get; set; }
    }
}
