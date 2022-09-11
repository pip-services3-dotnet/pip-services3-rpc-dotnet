using System.Threading.Tasks;
using PipServices3.Commons.Data;
using PipServices3.Commons.Refer;

namespace PipServices3.Rpc.Clients
{
    public class DummyDirectClient : DirectClient<IDummyController>, IDummyClient
    {
        public DummyDirectClient()
        {
            _dependencyResolver.Put("controller", new Descriptor("pip-services3-dummies", "controller", "*", "*", "*"));
        }

        public async Task<DataPage<Dummy>> GetPageByFilterAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            filter = filter ?? new FilterParams();
            paging = paging ?? new PagingParams();

            using (var timing = Instrument(correlationId, "dummy.get_page_by_filter"))
            {
                return await _controller.GetPageByFilterAsync(correlationId, filter, paging);
            }
        }

        public async Task<Dummy> GetOneByIdAsync(string correlationId, string id)
        {
            using (var timing = Instrument(correlationId, "dummy.get_one_by_id"))
            {
                return await _controller.GetOneByIdAsync(correlationId, id);
            }
        }

        public async Task<Dummy> CreateAsync(string correlationId, Dummy entity)
        {
            using (var timing = Instrument(correlationId, "dummy.create"))
            {
                return await _controller.CreateAsync(correlationId, entity);
            }
        }

        public async Task<Dummy> UpdateAsync(string correlationId, Dummy entity)
        {
            using (var timing = Instrument(correlationId, "dummy.update"))
            {
                return await _controller.UpdateAsync(correlationId, entity);
            }
        }

        public async Task<Dummy> DeleteByIdAsync(string correlationId, string id)
        {
            using (var timing = Instrument(correlationId, "dummy.delete_by_id"))
            {
                return await _controller.DeleteByIdAsync(correlationId, id);
            }
        }

        public async Task RaiseExceptionAsync(string correlationId)
        {
            await _controller.RaiseExceptionAsync(correlationId);
        }

        public async Task<string> CheckCorrelationId(string correlationId)
        {
            using (var timing = Instrument(correlationId, "dummy.check_correlation_id"))
            {
                return await _controller.CheckCorrelationId(correlationId);
            }
        }
    }
}
