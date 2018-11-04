using System.Threading.Tasks;

using PipServices3.Commons.Data;

namespace PipServices3.Rpc.Clients
{
    public sealed class DummyCommandableHttpClient : CommandableHttpClient, IDummyClient
    {
        public DummyCommandableHttpClient() 
            : base("dummy")
        { }

        public Task<DataPage<Dummy>> GetPageByFilterAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            filter = filter ?? new FilterParams();
            paging = paging ?? new PagingParams();

            var requestEntity = new
            {
                correlationId,
                filter,
                paging
            };

            return CallCommandAsync<DataPage<Dummy>>("get_dummies", correlationId, requestEntity);
        }

        public Task<Dummy> GetOneByIdAsync(string correlationId, string dummy_id)
        {
            var requestEntity = new
            {
                correlationId,
                dummy_id
            };

            return CallCommandAsync<Dummy>("get_dummy_by_id", correlationId, requestEntity);
        }

        public Task<Dummy> CreateAsync(string correlationId, Dummy dummy)
        {
            var requestEntity = new
            {
                correlationId,
                dummy
            };

            return CallCommandAsync<Dummy>("create_dummy", correlationId, requestEntity);
        }

        public Task<Dummy> UpdateAsync(string correlationId, Dummy dummy)
        {
            var requestEntity = new
            {
                correlationId,
                dummy
            };

            return CallCommandAsync<Dummy>("update_dummy", correlationId, requestEntity);
        }

        public Task<Dummy> DeleteByIdAsync(string correlationId, string dummy_id)
        {
            var requestEntity = new
            {
                correlationId,
                dummy_id
            };

            return CallCommandAsync<Dummy>("delete_dummy", correlationId, requestEntity);
        }

        public Task RaiseExceptionAsync(string correlationId)
        {
            return CallCommandAsync<object>("raise_exception", correlationId, null);
        }
    }
}
