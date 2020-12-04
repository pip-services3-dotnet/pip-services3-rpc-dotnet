using PipServices3.Commons.Commands;
using PipServices3.Commons.Config;
using PipServices3.Commons.Convert;
using PipServices3.Commons.Validate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeCode = PipServices3.Commons.Convert.TypeCode;

namespace PipServices3.Rpc.Services
{
    public class CommandableSwaggerDocument
    {
        public List<ICommand> Commands { get; set; }

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

        public CommandableSwaggerDocument(string baseRoute, ConfigParams config, List<ICommand> commands)
        {
            BaseRoute = baseRoute;
            Commands = commands ?? new List<ICommand>();

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
            foreach (ICommand command in Commands)
            {
                var path = string.Format("{0}/{1}", BaseRoute, command.Name);
                if (!path.StartsWith("/")) path = "/" + path;

                data.Add(path, new Dictionary<string, object>
                {
                    {   "post", new Dictionary<string, object>
                        {
                            {   "tags", new List<string>(new[] { BaseRoute })},
                            {   "operationId", command.Name },
                            {   "requestBody", CreateRequestBodyData(command) },
                            {   "responses", CreateResponsesData() }
                        }
                    }
                });
            }

            return data;
        }

        private Dictionary<string, object> CreateRequestBodyData(ICommand command)
        {
            var schemaData = CreateSchemaData(command);
            return schemaData == null ? null : new Dictionary<string, object>
            {
                {   "content", new Dictionary<string, object>
                    {
                        {   "application/json", new Dictionary<string, object>
                            {
                                {   "schema", schemaData }
                            }
                        }
                    }
                }
            };
        }

        private Dictionary<string, object> CreateSchemaData(ICommand command)
        {
            var schema = command.Schema as ObjectSchema;

            if (schema == null || schema.Properties == null)
                return null;

            return CreatePropertyData(schema, true);
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
                    default:
                        return new Dictionary<string, object>
                        {
                            { "type", TypeConverter.ToString(typeCode) }
                        };
                }
            }
        }

        private Dictionary<string, object> CreateResponsesData()
        {
            return new Dictionary<string, object>
            {
                {   "200", new Dictionary<string, object>
                    {
                        {   "description", "Successful response" },
                        {   "content", new Dictionary<string, object>
                            {
                                { "application/json", new Dictionary<string, object>
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
                    }
                }
            };
        }

        protected void WriteData(int indent, Dictionary<string, object> data)
        {
            foreach (var key in data.Keys)
            {
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
                else if (value is string str)
                {
                    WriteAsString(indent, key, str);
                }
                else
                {
                    WriteAsObject(indent, key, value);
                }
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
