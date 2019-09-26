using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using PipServices3.Commons.Config;
using PipServices3.Commons.Run;
using PipServices3.Commons.Convert;
using PipServices3.Commons.Data;
using PipServices3.Commons.Reflect;

namespace PipServices3.Rpc.Services
{
  public class RestOperationParameters: AnyValueMap
  {
    public RestOperationParameters()
    {
    }

    public RestOperationParameters(IDictionary values)
      : base(values)
    {
    }

    public string RequestBody = null;
    public IFormFileCollection RequestFiles = null;

    public override object Get(string path)
    {
      if (string.IsNullOrEmpty(path))
        return (object) null;
      if (path.IndexOf(".", StringComparison.Ordinal) > 0)
        return RecursiveObjectReader.GetProperty((object) this, path);
      return base.Get(path);
    }

    public override void Set(string path, object value)
    {
      if (string.IsNullOrWhiteSpace(path))
        return;
      if (path.IndexOf(".", StringComparison.Ordinal) > 0)
        RecursiveObjectWriter.SetProperty((object) this, path, value);
      else
        base.Set(path, value);
    }

    public RestOperationParameters GetAsNullableParameters(string key)
    {
      AnyValueMap asNullableMap = this.GetAsNullableMap(key);
      if (asNullableMap == null)
        return (RestOperationParameters) null;
      return new RestOperationParameters((IDictionary) asNullableMap);
    }

    public RestOperationParameters GetAsParameters(string key)
    {
      return new RestOperationParameters((IDictionary) this.GetAsMap(key));
    }

    public RestOperationParameters GetAsParametersWithDefault(string key, RestOperationParameters defaultValue)
    {
      return this.GetAsNullableParameters(key) ?? defaultValue;
    }

    public new bool ContainsKey(string key)
    {
      return RecursiveObjectReader.HasProperty((object) this, key);
    }

    public RestOperationParameters Override(RestOperationParameters parameters)
    {
      return this.Override(parameters, false);
    }

    public RestOperationParameters Override(RestOperationParameters parameters, bool recursive)
    {
      RestOperationParameters parameters1 = new RestOperationParameters();
      if (recursive)
      {
        RecursiveObjectWriter.CopyProperties((object) parameters1, (object) this);
        RecursiveObjectWriter.CopyProperties((object) parameters1, (object) parameters);
      }
      else
      {
        ObjectWriter.SetProperties((object) parameters1, (IDictionary<string, object>) this);
        ObjectWriter.SetProperties((object) parameters1, (IDictionary<string, object>) parameters);
      }
      return parameters1;
    }

    public RestOperationParameters SetDefaults(RestOperationParameters defaultParameters)
    {
      return this.SetDefaults(defaultParameters, false);
    }

    public RestOperationParameters SetDefaults(RestOperationParameters defaultParameters, bool recursive)
    {
      RestOperationParameters parameters = new RestOperationParameters();
      if (recursive)
      {
        RecursiveObjectWriter.CopyProperties((object) parameters, (object) defaultParameters);
        RecursiveObjectWriter.CopyProperties((object) parameters, (object) this);
      }
      else
      {
        ObjectWriter.SetProperties((object) parameters, (IDictionary<string, object>) defaultParameters);
        ObjectWriter.SetProperties((object) parameters, (IDictionary<string, object>) this);
      }
      return parameters;
    }

    public void AssignTo(object value)
    {
      if (value == null || this.Count == 0)
        return;
      RecursiveObjectWriter.CopyProperties(value, (object) this);
    }

    public RestOperationParameters Pick(params string[] paths)
    {
      RestOperationParameters parameters = new RestOperationParameters();
      foreach (string path in paths)
      {
        if (this.ContainsKey(path))
          parameters[path] = this.Get(path);
      }
      return parameters;
    }

    public RestOperationParameters Omit(params string[] paths)
    {
      RestOperationParameters parameters = new RestOperationParameters();
      foreach (string path in paths)
        parameters.Remove(path);
      return parameters;
    }

    public string ToJson()
    {
      return JsonConverter.ToJson((object) this);
    }

    public static RestOperationParameters FromTuples(params object[] tuples)
    {
      return new RestOperationParameters((IDictionary) AnyValueMap.FromTuples(tuples));
    }

    public static RestOperationParameters MergeParams(params RestOperationParameters[] parameters)
    {
      return new RestOperationParameters((IDictionary) AnyValueMap.FromMaps((IDictionary[]) parameters));
    }

    public static RestOperationParameters FromJson(string json)
    {
      IDictionary<string, object> nullableMap = JsonConverter.ToNullableMap(json);
      if (nullableMap == null)
        return new RestOperationParameters();
      return new RestOperationParameters((IDictionary) nullableMap);
    }

    public static RestOperationParameters FromConfig(ConfigParams config)
    {
      if (config == null)
        return new RestOperationParameters();
      return new RestOperationParameters((IDictionary) config);
    }
  }
}