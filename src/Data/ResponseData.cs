using PipServices3.Commons.Validate;

namespace PipServices3.Rpc.Data
{
    public class ResponseData
    {
        public int StatusCode { get; set; }
        public string Description { get; set; }
        public object Schema { get; set; }
    }
}
