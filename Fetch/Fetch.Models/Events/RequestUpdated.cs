using Fetch.Models.Data;

namespace Fetch.Models.Events
{
    public class RequestUpdated
    {
        public int RequestId { get; set; }
        public Status NewStatus { get; set; }
        
        /// <summary>
        /// The provider whose bid the customer accepted, if it's an accept. 
        /// </summary>
        public int? ProviderId { get; set; }
    }
}
