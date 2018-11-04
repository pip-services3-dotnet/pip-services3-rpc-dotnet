
using PipServices3.Commons.Convert;
using PipServices3.Commons.Validate;

namespace PipServices3.Rpc
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
