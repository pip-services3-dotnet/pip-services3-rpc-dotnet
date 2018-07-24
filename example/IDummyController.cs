using PipServices.Commons.Data;
using System.Threading.Tasks;

namespace PipServices.Rpc
{
    public interface IDummyController
    {
        DataPage<Dummy> GetPageByFilter(string correlationId, FilterParams filter, PagingParams paging);
        Dummy GetOneById(string correlationId, string id);
        Dummy Create(string correlationId, Dummy entity);
        Dummy Update(string correlationId, Dummy entity);
        Dummy DeleteById(string correlationId, string id);
        void RaiseException(string correlationId);

        Task<bool> PingAsync();
    }
}
