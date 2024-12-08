using Fetch.Models.Data;

namespace Fetch.Models.Events
{
    public class RequestCreated
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public ServiceCategories Category { get; set; }
    }
}
