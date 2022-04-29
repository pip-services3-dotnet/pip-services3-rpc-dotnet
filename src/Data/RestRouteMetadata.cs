using PipServices3.Commons.Convert;
using PipServices3.Commons.Validate;

using System.Collections.Generic;

namespace PipServices3.Rpc.Data
{
    public class RestRouteMetadata
    {
        public string Method { get; set; }
        public string Route { get; set; }
        public string[] Tags { get; set; }
        public List<QueryParam> QueryParams { get; set; } = new List<QueryParam>();
        public ObjectSchema BodySchema { get; set; }
        public bool NeedsFile { get; set; }
        public List<ResponseData> Responses { get; set; } = new List<ResponseData>();
        public string Authentication { get; set; }

        public RestRouteMetadata SetsMethodAndRoute(string method, string route)
        {
            Method = method;
            Route = route;
            return this;
        }

        public RestRouteMetadata SetsTags(params string[] tags)
        {
            Tags = tags;
            return this;
        }

        public RestRouteMetadata ExpectsHeader()
        {

            return this;
        }

        public RestRouteMetadata ReceivesQueryParam(string name, TypeCode typeCode, bool required = false, object defaultValue = null, string description = null)
        {
            QueryParams.Add(new QueryParam
            {
                Name = name,
                TypeCode = typeCode,
                Required = required,
                DefaultValue = defaultValue,
                Description = description
            });

            return this;
        }

        public RestRouteMetadata ReceivesOptionalQueryParam(string name, TypeCode typeCode, object defaultValue = null, string description = null)
        {
            return ReceivesQueryParam(name, typeCode, false, defaultValue, description);
        }

        public RestRouteMetadata ReceivesRequiredQueryParam(string name, TypeCode typeCode, object defaultValue = null, string description = null)
        {
            return ReceivesQueryParam(name, typeCode, true, defaultValue, description);
        }

        public RestRouteMetadata ReceivesCorrelationIdParam()
        {
            return ReceivesOptionalQueryParam("correlation_id", TypeCode.String);
        }

        public RestRouteMetadata ReceivesBodyFromSchema(ObjectSchema schema)
        {
            BodySchema = schema;

            return this;
        }

        public RestRouteMetadata ReceivesFile()
        {
            NeedsFile = true;

            return this;
        }

        public RestRouteMetadata SendsData(int statusCode, string description, object schema = null)
        {
            Responses.Add(new ResponseData
            {
                StatusCode = statusCode,
                Description = description,
                Schema = schema
            });

            return this;
        }

        public RestRouteMetadata SendsData200(object schema = null)
        {
            return SendsData(200, "Success", schema);
        }

        public RestRouteMetadata SendsDataPage200(object schema)
        {
            return SendsData(200, "Success", new ObjectSchema()
                .WithRequiredProperty("total", TypeCode.Long)
                .WithRequiredProperty("data", new ArraySchema(schema))
                );
        }

        public RestRouteMetadata SendsData400(object schema = null)
        {
            return SendsData(400, "Bad request", schema);
        }

        public RestRouteMetadata UsesBasicAuthentication()
        {
            Authentication = "basic";
            return this;
        }

        public RestRouteMetadata UsesBearerAuthentication()
        {
            Authentication = "bearer";
            return this;
        }
    }
}
