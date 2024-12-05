namespace Fetch.Models.Data
{
    public class Ticket
    {
        public int Id { get; set; }
        /// <summary>
        /// The title of the request
        /// </summary>
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTimeOffset DueDate { get; set; }
    }
}
