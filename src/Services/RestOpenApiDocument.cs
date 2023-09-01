using PipServices3.Commons.Config;
using PipServices3.Commons.Convert;
using PipServices3.Commons.Validate;
using PipServices3.Rpc.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeCode = PipServices3.Commons.Convert.TypeCode;

namespace PipServices3.Rpc.Services
{
    public class RestOpenApiDocument
    {
        public List<RestRouteMetadata> Commands { get; set; }

        public string Version { get; set; } = "3.0.2";
        public string BaseRoute { get; set; }

        public string InfoTitle { get; set; }
        public string InfoDescription { get; set; }
        public string InfoVersion { get; set; } = "1";
        public string InfoTermsOfService { get; set; }

        public string InfoContactName { get; set; }
        public string InfoContactUrl { get; set; }
        public string InfoContactEmail { get; set; }

        public string InfoLicenseName { get; set; }
        public string InfoLicenseUrl { get; set; }

        protected readonly StringBuilder _builder = new StringBuilder();
        protected readonly Dictionary<string, object> _objectType = new Dictionary<string, object>
        {
            { "type", "object" }
        };

        private Dictionary<string, bool> _auth = new Dictionary<string, bool>();

        public RestOpenApiDocument(string baseRoute, ConfigParams config, List<RestRouteMetadata> commands)
        {
            BaseRoute = baseRoute;
            Commands = commands ?? new List<RestRouteMetadata>();

            config = config ?? new ConfigParams();

            InfoTitle = config.GetAsStringWithDefault("name", "CommandableHttpService");
            InfoDescription = config.GetAsStringWithDefault("description", "Commandable microservice");
        }

        public override string ToString()
        {
            var data = new Dictionary<string, object>
            {
                {   "openapi", Version },
                {   "info", new Dictionary<string, object>
                    {
                        {   "title", InfoTitle },
                        {   "description", InfoDescription },
                        {   "version", InfoVersion },
                        {   "termsOfService", InfoTermsOfService },
                        {   "contact", new Dictionary<string, object>
                            {
                                { "name", InfoContactName },
                                { "url", InfoContactUrl },
                                { "email", InfoContactEmail },
                            }
                        },
                        {   "license", new Dictionary<string, object>
                            {
                                { "name", InfoLicenseName },
                                { "url", InfoLicenseUrl },
                            }
                        },
                    }
                },
                {   "paths", CreatePathsData() },
                {   "components", CreateComponentsData() }
            };

            WriteData(0, data);

            return _builder.ToString();
        }

        private Dictionary<string, object> CreatePathsData()
        {
            var data = new Dictionary<string, object>();

            foreach (var routeGroup in Commands.GroupBy(g => g.Route))
            {
                var path = string.Format("{0}{1}", BaseRoute, routeGroup.Key.StartsWith("/") ? routeGroup.Key : $"/{routeGroup.Key}");
                if (!path.StartsWith("/")) path = "/" + path;

                var pathData = new Dictionary<string, object>();
                var pathParamsData = CreatePathParametersData(path);

                foreach (var metadata in routeGroup)
                {
                    var tags = metadata.Tags?.ToList() ?? new List<string>();

                    if (!tags.Any())
                    {
                        tags.Add(BaseRoute ?? metadata.Route);
                    }

                    var methodData = new Dictionary<string, object>
                    {
                        {   "tags", tags },
                        {   "operationId", $"{metadata.Route}-{metadata.Method}" },
                    };

                    var paramsData = new List<Dictionary<string, object>>(pathParamsData);
                    paramsData.AddRange(CreateParametersData(metadata.QueryParams));
                    if (paramsData.Any())
                    {
                        methodData.Add("parameters", paramsData);
                    }

                    if (metadata.BodySchema != null)
                    {
                        var bodyData = CreateSchemaContentData(metadata.BodySchema, true);
                        if (bodyData != null && bodyData.Any())
                        {
                            methodData.Add("requestBody", bodyData);
                        }
                    }
                    else if (metadata.NeedsFile)
                    {
                        methodData.Add("requestBody", CreateFileContentData());
                    }

                    methodData.Add("responses", CreateResponsesData(metadata.Responses));

                    if (!string.IsNullOrWhiteSpace(metadata.Authentication))
                    {
                        methodData.Add("security", new Dictionary<string, object>
                        {
                            {   $"- {metadata.Authentication}Auth", "[]" }
                        });

                        _auth[metadata.Authentication] = true;
                    }

                    pathData.Add(metadata.Method.ToLower(), methodData);
                }

                data.Add(path, pathData);
            }

            return data;
        }

        private List<Dictionary<string, object>> CreatePathParametersData(string route)
        {
            var data = new List<Dictionary<string, object>>();

            if (route.Contains("{") && route.Contains("}"))
            {
                var splitRoute = route.Split('/');

                for (var i = 0; i < splitRoute.Length; i++)
                {
                    var r = splitRoute[i];
                    if (r.StartsWith("{") && r.EndsWith("}"))
                    {
                        var key = r.Substring(1).Substring(0, r.Length - 2);

                        data.Add(new Dictionary<string, object>
                        {
                            {   "name", key },
                            {   "in", "path" },
                            {   "schema", new Dictionary<string, object>
                                {
                                    {   "type", "string" }
                                }
                            },
                            {   "required", true },
                        });
                    }
                }
            }

            return data;
        }

        private List<Dictionary<string, object>> CreateParametersData(List<QueryParam> queryParams)
        {
            var data = new List<Dictionary<string, object>>();

            foreach (var item in queryParams)
            {
                var schemaData = CreatePropertyTypeData(item.TypeCode);

                if (item.DefaultValue != null)
                {
                    schemaData.Add("default", item.DefaultValue);
                }

                var dataItem = new Dictionary<string, object>
                {
                    {   "name", item.Name },
                    {   "in", "query" },
                    {   "schema", schemaData }
                };

                if (item.Required)
                    dataItem.Add("required", true);

                if (!string.IsNullOrWhiteSpace(item.Description))
                    dataItem.Add("description", item.Description);

                data.Add(dataItem);
            }

            return data;
        }

        private Dictionary<string, object> CreateSchemaContentData(object schema, bool includeRequired)
        {
            if (schema == null)
            {
                return new Dictionary<string, object>
                {
                    {   "content", new Dictionary<string, object>
                        {
                            {   "application/json", new Dictionary<string, object>
                                {
                                    {   "schema", new Dictionary<string, object>
                                        {
                                            {   "type", "object" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
            }

            return new Dictionary<string, object>
            {
                {   "content", new Dictionary<string, object>
                    {
                        {   "application/json", new Dictionary<string, object>
                            {
                                {   "schema", CreatePropertyData(schema, includeRequired) }
                            }
                        }
                    }
                }
            };
        }

        private Dictionary<string, object> CreateFileContentData()
        {
            return new Dictionary<string, object>
            {
                {   "content", new Dictionary<string, object>
                    {
                        {   "multipart/form-data", new Dictionary<string, object>
                            {
                                {   "schema", new Dictionary<string, object>
                                    {
                                        {   "type", "object" },
                                        {   "properties", new Dictionary<string, object>
                                            {
                                                {   "filename", new Dictionary<string, object>
                                                    {
                                                        {   "type", "array" },
                                                        {   "items", new Dictionary<string, object>
                                                            {
                                                                { "type", "string" },
                                                                { "format", "binary" }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private Dictionary<string, object> CreatePropertyData(object @object, bool includeRequired)
        {
            if (!(@object is ObjectSchema))
            {
                return CreatePropertyTypeData(@object);
            }

            var schema = @object as ObjectSchema;

            if (schema.Properties == null)
            {
                return _objectType;
            }

            var properties = new Dictionary<string, object>();
            var required = new List<string>();

            if (schema.Properties == null)
            {
                return new Dictionary<string, object>
                {
                    { "properties", _objectType }
                };
            }

            foreach (var property in schema.Properties)
            {
                if (property.Type == null)
                {
                    properties.Add(property.Name, _objectType);
                    continue;
                }

                var propertyName = property.Name;
                var propertyType = property.Type;

                properties.Add(propertyName, CreatePropertyTypeData(propertyType));

                if (includeRequired && property.IsRequired) required.Add(propertyName);
            }

            var data = new Dictionary<string, object>
            {
                { "properties", properties }
            };

            if (required.Count > 0)
            {
                data.Add("required", required);
            }

            return data;
        }

        private Dictionary<string, object> CreatePropertyTypeData(object property)
        {
            var propertyType = property.GetType();

            //ObjectSchema
            if (property is ObjectSchema)
            {
                return _objectType
                    .Union(CreatePropertyData((ObjectSchema)property, false))
                    .ToDictionary(k => k.Key, o => o.Value);
            }
            //ArraySchema
            else if (property is ArraySchema)
            {
                var itemType = ((ArraySchema)property).ValueType;
                return new Dictionary<string, object>
                {
                    { "type", "array" },
                    { "items", itemType == null ? _objectType : CreatePropertyTypeData(itemType) }
                };
            }
            //List<>
            else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var itemType = propertyType.GetGenericArguments()[0];
                return new Dictionary<string, object>
                {
                    { "type", "array" },
                    { "items", CreatePropertyTypeData(itemType) }
                };
            }
            //Dictionary<,>
            else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var keyType = propertyType.GetGenericArguments()[0];
                var valueType = propertyType.GetGenericArguments()[1];
                return new Dictionary<string, object>
                {
                    { "type", "object" },
                    { "additionalProperties", CreatePropertyTypeData(valueType) }
                };
            }
            //MapSchema
            else if (property is MapSchema)
            {
                var mapSchema = property as MapSchema;
                var keyType = mapSchema.KeyType;
                var valueType = mapSchema.ValueType;
                return new Dictionary<string, object>
                {
                    { "type", "object" },
                    { "additionalProperties", CreatePropertyTypeData(valueType) }
                };
            }
            //array (e.g. new string[] { })
            else if (propertyType.IsArray)
            {
                return new Dictionary<string, object>
                {
                    { "type", "array" },
                    { "items", CreatePropertyTypeData(propertyType.GetElementType()) }
                };
            }
            else
            {
                var typeCode = TypeCode.Object;

                if (property is TypeCode)
                {
                    typeCode = (TypeCode)property;
                }
                else
                {
                    Type type = property as Type;
                    if (type != null && type.Equals(typeof(byte)))
                    {
                        typeCode = TypeCode.Integer;
                    }
                    else
                    {
                        typeCode = TypeConverter.ToTypeCode(type);
                    }
                }

                typeCode = typeCode == TypeCode.Unknown ? TypeCode.Object : typeCode;

                switch (typeCode)
                {
                    case TypeCode.Integer:
                        return new Dictionary<string, object>
                        {
                            { "type", "integer" },
                            { "format", "int32" }
                        };
                    case TypeCode.Long:
                        return new Dictionary<string, object>
                        {
                            { "type", "integer" },
                            { "format", "int64" }
                        };
                    case TypeCode.Float:
                        return new Dictionary<string, object>
                        {
                            { "type", "number" },
                            { "format", "float" }
                        };
                    case TypeCode.Double:
                        return new Dictionary<string, object>
                        {
                            { "type", "number" },
                            { "format", "double" }
                        };
                    case TypeCode.DateTime:
                        return new Dictionary<string, object>
                        {
                            { "type", "string" },
                            { "format", "date-time" }
                        };
                    case TypeCode.Map:
                        return new Dictionary<string, object>
                        {
                            { "type", "object" },
                            { "additionalProperties", _objectType }
                        };
                    case TypeCode.Array:
                        return new Dictionary<string, object>
                        {
                            { "type", "array" },
                            { "items", _objectType }
                        };
                    default:
                        return new Dictionary<string, object>
                        {
                            { "type", TypeConverter.ToString(typeCode) }
                        };
                }
            }
        }

        private Dictionary<string, object> CreateResponsesData(List<ResponseData> responses)
        {
            var data = new Dictionary<string, object>();
            foreach (var item in responses)
            {
                var respData = new Dictionary<string, object>
                {
                    {   "description", item.Description ?? "Success" }
                };
                data.Add(item.StatusCode.ToString(),
                    respData
                    .Union(CreateSchemaContentData(item.Schema, false))
                    .ToDictionary(k => k.Key, v => v.Value));
            }

            return data;
        }

        private Dictionary<string, object> CreateComponentsData()
        {
            if (!_auth.Any())
                return null;

            var data = new Dictionary<string, object>();

            foreach (var item in _auth)
            {
                data.Add($"{item.Key}Auth", new Dictionary<string, object>
                {
                    { "type", "http" },
                    { "scheme", item.Key }
                });
            }

            return new Dictionary<string, object>
            {
                {   "securitySchemes", data }
            };
        }

        protected void WriteData(int indent, Dictionary<string, object> data, bool witFirsthHyphen = false)
        {
            var tmp = indent;
            bool firstStep = true;

            foreach (var key in data.Keys)
            {
                if (witFirsthHyphen && firstStep)
                {
                    WriteHyphen(indent);
                    indent = 0;
                    firstStep = false;
                    tmp++;
                }

                var value = data[key];

                if (value is List<string> list)
                {
                    if (list.Count > 0)
                    {
                        WriteName(indent, key);
                        foreach (var item in list)
                        {
                            WriteArrayItem(indent + 1, item);
                        }
                    }
                }
                else if (value is Dictionary<string, object> dict)
                {
                    if (dict.Any(x => x.Value != null))
                    {
                        WriteName(indent, key);
                        WriteData(indent + 1, dict);
                    }
                }
                else if (value is List<Dictionary<string, object>> dictList)
                {
                    WriteName(indent, key);
                    foreach (var dictItem in dictList)
                    {
                        if (dictItem.Any(x => x.Value != null))
                        {
                            WriteData(indent + 1, dictItem, true);
                        }
                    }
                }
                else if (value is string str)
                {
                    WriteAsString(indent, key, str);
                }
                else
                {
                    WriteAsObject(indent, key, value);
                }

                indent = tmp;
            }
        }

        protected void WriteName(int indent, string name)
        {
            var spaces = GetSpaces(indent);
            _builder.Append(spaces).Append(name).AppendLine(":");
        }

        protected void WriteArrayItem(int indent, string name, bool isObjectItem = false)
        {
            var spaces = GetSpaces(indent);
            _builder.Append(spaces).Append("- ");

            if (isObjectItem) _builder.Append(name).AppendLine(":");
            else _builder.AppendLine(name);
        }

        protected void WriteHyphen(int indent)
        {
            var spaces = GetSpaces(indent);
            _builder.Append(spaces).Append("- ");
        }

        protected void WriteAsObject(int indent, string name, object value)
        {
            if (value == null) return;

            var spaces = GetSpaces(indent);
            _builder.Append(spaces).Append(name).Append(": ").Append(value).AppendLine();
        }

        protected void WriteAsString(int indent, string name, string value)
        {
            if (value == null) return;

            var spaces = GetSpaces(indent);
            if (!value.Equals("[]")) _builder.Append(spaces).Append(name).Append(": '").Append(value).AppendLine("'");
            else _builder.Append(spaces).Append(name).Append(": ").Append(value).AppendLine("");
        }

        protected string GetSpaces(int length)
        {
            return new string(' ', length * 2);
        }
    }
}
