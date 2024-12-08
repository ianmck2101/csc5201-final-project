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
        bool AcceptRequest(int id, int providerId);
        bool CancelRequest(int id);
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
            var newId = _dal.CreateNewRequest(request);

            var requestCreatedEvent = new RequestCreated()
            {
                Id = newId,
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                Category = request.Category,
            };

            _kafkaProducer.ProduceNewRequestMessageAsync(JsonSerializer.Serialize(requestCreatedEvent));
        }

        public bool DeleteRequest(int id)
        {
            var updateRequestEvent = new RequestUpdated()
            {
                RequestId = id,
                NewStatus = Status.Closed
            };

            _kafkaProducer.ProduceRequestUpdatedMessageAsync(JsonSerializer.Serialize(updateRequestEvent));

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

        public bool AcceptRequest(int requestId, int providerId)
        {
            var request = _dal.GetRequest(requestId);

            if (request == null)
            {
                return false;
            }

            var updateRequestEvent = new RequestUpdated()
            {
                RequestId = request.Id,
                NewStatus = Status.Accepted, 
                ProviderId = providerId
            };

            _kafkaProducer.ProduceRequestUpdatedMessageAsync(JsonSerializer.Serialize(updateRequestEvent));

            return true;
        }

        public bool CancelRequest(int id)
        {
            var request = _dal.GetRequest(id);

            if (request == null)
            {
                return false;
            }

            var updateRequestEvent = new RequestUpdated()
            {
                RequestId = request.Id,
                NewStatus = Status.Closed
            };

            _kafkaProducer.ProduceRequestUpdatedMessageAsync(JsonSerializer.Serialize(updateRequestEvent));

            return true;
        }
    }
}
