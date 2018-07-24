using System.Threading.Tasks;
using PipServices.Commons.Data;
using PipServices.Commons.Refer;

namespace PipServices.Rpc.Clients
{
    public class DummyDirectClient : DirectClient<IDummyController>, IDummyClient
    {
        public DummyDirectClient()
        {
            _dependencyResolver.Put("controller", new Descriptor("pip-services-dummies", "controller", "*", "*", "*"));
        }

        public Task<DataPage<Dummy>> GetPageByFilterAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            filter = filter ?? new FilterParams();
            paging = paging ?? new PagingParams();

            using (var timing = Instrument(correlationId, "dummy.get_page_by_filter"))
            {
                return Task.FromResult(_controller.GetPageByFilter(correlationId, filter, paging));
            }
        }

        public Task<Dummy> GetOneByIdAsync(string correlationId, string id)
        {
            using (var timing = Instrument(correlationId, "dummy.get_one_by_id"))
            {
                return Task.FromResult(_controller.GetOneById(correlationId, id));
            }
        }

        public Task<Dummy> CreateAsync(string correlationId, Dummy entity)
        {
            using (var timing = Instrument(correlationId, "dummy.create"))
            {
                return Task.FromResult(_controller.Create(correlationId, entity));
            }
        }

        public Task<Dummy> UpdateAsync(string correlationId, Dummy entity)
        {
            using (var timing = Instrument(correlationId, "dummy.update"))
            {
                return Task.FromResult(_controller.Update(correlationId, entity));
            }
        }

        public Task<Dummy> DeleteByIdAsync(string correlationId, string id)
        {
            using (var timing = Instrument(correlationId, "dummy.delete_by_id"))
            {
                return Task.FromResult(_controller.DeleteById(correlationId, id));
            }
        }

        public Task RaiseException(string correlationId)
        {
            _controller.RaiseException(correlationId);
            return Task.Delay(0);
        }
    }
}
