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
                {   "paths", CreatePathsData() }
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
                    var tags = metadata.Tags.ToList();

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

                    methodData.Add("responses", CreateResponsesData(metadata.Responses));

                    pathData.Add(metadata.Method, methodData);
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

        private Dictionary<string, object> CreateSchemaContentData(ObjectSchema schema, bool includeRequired)
        {
            if (schema == null || schema.Properties == null)
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

        private Dictionary<string, object> CreatePropertyData(ObjectSchema schema, bool includeRequired)
        {
            var properties = new Dictionary<string, object>();
            var required = new List<string>();

            foreach (var property in schema.Properties)
            {
                if (property.Type == null)
                {
                    properties.Add(property.Name, _objectType);
                    continue;
                }

                var propertyName = property.Name;
                var propertyType = property.Type;

                if (propertyType is ArraySchema)
                {
                    properties.Add(propertyName, new Dictionary<string, object>
                    {
                        { "type", "array" },
                        { "items", CreatePropertyTypeData(((ArraySchema)propertyType).ValueType) }
                    });
                }
                else
                {
                    properties.Add(propertyName, CreatePropertyTypeData(propertyType));
                }

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

        private Dictionary<string, object> CreatePropertyTypeData(object propertyType)
        {
            if (propertyType is ObjectSchema)
            {
                return _objectType
                    .Union(CreatePropertyData((ObjectSchema)propertyType, false))
                    .ToDictionary(k => k.Key, o => o.Value);
            }
            else
            {
                var typeCode = (propertyType is TypeCode) ? (TypeCode)propertyType : TypeConverter.ToTypeCode(propertyType as Type);
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
                            { "type", "string" },
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
                else if (value is List<Tuple<string, object>> tuple)
                {
                    WriteName(indent, key);
                    indent++;
                    foreach (var item in tuple)
                    {
                        if (item.Item2 != null && item.Item2 is Dictionary<string, object> tuple_dict)
                        {
                            WriteName(indent, item.Item1);
                            WriteData(indent + 1, tuple_dict);
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
            _builder.Append(spaces).Append(name).Append(": '").Append(value).AppendLine("'");
        }

        protected string GetSpaces(int length)
        {
            return new string(' ', length * 2);
        }
    }
}
