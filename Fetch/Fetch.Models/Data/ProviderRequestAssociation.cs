namespace Fetch.Models.Data
{
    public class ProviderRequestAssociation
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public Status Status { get; set; }
        public int ProviderId { get; set; }
    }
}

