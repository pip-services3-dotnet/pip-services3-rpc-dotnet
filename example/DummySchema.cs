
using PipServices.Commons.Convert;
using PipServices.Commons.Validate;

namespace PipServices.Rpc
{
    public class DummySchema : ObjectSchema
    {
        public DummySchema()
        {
            WithOptionalProperty("id", TypeCode.String);
            WithRequiredProperty("key", TypeCode.String);
            WithOptionalProperty("content", TypeCode.String);
            WithOptionalProperty("flag", TypeCode.Boolean);
        }
    }
}
