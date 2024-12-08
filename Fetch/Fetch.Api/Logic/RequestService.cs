using System.Text.Json;
using Fetch.Api.Data;
using Fetch.Models.Data;
using Fetch.Models.Events;

namespace Fetch.Api.Logic
{
    public interface IRequestService
    {
        void CreateNewRequest(BaseRequest request);
        bool DeleteRequest(int id);
        BaseRequest? GetRequest(int id);
        IEnumerable<BaseRequest> GetAllRequests();
    }

    public class RequestService : IRequestService
    {
        private readonly IRequestDAL _dal;
        private readonly IKafkaProducer _kafkaProducer;

        public RequestService(IRequestDAL requestDAL, IKafkaProducer kafkaProducer)
        {
            _dal = requestDAL ?? throw new ArgumentNullException(nameof(requestDAL));
            _kafkaProducer = kafkaProducer ?? throw new ArgumentNullException(nameof(kafkaProducer));

            _dal.EnsureTablesExist();
        }

        public void CreateNewRequest(BaseRequest request)
        {
            _dal.CreateNewRequest(request);

            var requestCreatedEvent = new RequestCreated()
            {
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                Category = request.Category,
            };

            _kafkaProducer.ProduceNewRequestMessageAsync(JsonSerializer.Serialize(requestCreatedEvent));
        }

        public bool DeleteRequest(int id)
        {
            return _dal.DeleteRequest(id);
        }

        public IEnumerable<BaseRequest> GetAllRequests()
        {
            return _dal.GetAllRequests();
        }

        public BaseRequest? GetRequest(int id)
        {
            return _dal.GetRequest(id);
        }
    }
}
