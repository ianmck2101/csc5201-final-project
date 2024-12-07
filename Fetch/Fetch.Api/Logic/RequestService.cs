using Fetch.Api.Data;
using Fetch.Models.Data;

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

        public RequestService(IRequestDAL requestDAL)
        {
            _dal = requestDAL ?? throw new ArgumentNullException(nameof(requestDAL));
            _dal.EnsureTablesExist();
        }

        public void CreateNewRequest(BaseRequest request)
        {
            _dal.CreateNewRequest(request);
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
