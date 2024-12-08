namespace Fetch.Models.Data
{
    public class Provider
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public IEnumerable<ServiceCategories> Categories { get; set; }
    }
}
