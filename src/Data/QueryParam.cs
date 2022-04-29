using PipServices3.Commons.Convert;

namespace PipServices3.Rpc.Data
{
    public class QueryParam
    {
        public string Name { get; set; }
        public TypeCode TypeCode { get; set; }
        public bool Required { get; set; }
        public object DefaultValue { get; set; }
        public string Description { get; set; }
    }
}
