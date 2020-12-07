using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using PipServices3.Commons.Data;
using System;
using System.IO;

namespace PipServices3.Rpc.Services
{
	/// <summary>
	/// Helper class that handles HTTP-based requests.
	/// </summary>
	public static class HttpRequestHelper
	{

        public static string GetCorrelationId(HttpRequest request)
        {
            return request.Query.TryGetValue("correlation_id", out StringValues correlationId)
                ? correlationId.ToString()
                : null;
        }

        public static FilterParams GetFilterParams(HttpRequest request)
        {
            var filter = new FilterParams();
            var parser = FilterParams.FromString(ExtractFromQuery("filter", request));

            foreach (var filterParam in parser)
            {
                filter.Set(filterParam.Key, filterParam.Value);
            }

            return filter;
        }

        public static PagingParams GetPagingParams(HttpRequest request)
        {
            var paging = PagingParams.FromTuples(
                "total", ExtractFromQuery("total", request),
                "skip", ExtractFromQuery("skip", request),
                "take", ExtractFromQuery("take", request)
            );
            return paging;
        }

        public static RestOperationParameters GetParameters(HttpRequest request)
        {
            string body;

            var formCollection = request.HasFormContentType ? request.ReadFormAsync().Result : null;

            using (var streamReader = new StreamReader(request.Body))
            {
                body = streamReader.ReadToEnd();
            }

            var parameters = string.IsNullOrEmpty(body)
                ? new RestOperationParameters() : RestOperationParameters.FromBody(body);

            parameters.RequestBody = body;
            parameters.RequestFiles = formCollection?.Files;

            foreach (var pair in request.Query)
            {
                parameters.QueryParameters.Set(pair.Key, pair.Value[0]);
                parameters.Set(pair.Key, pair.Value[0]);
            }

            foreach (var pair in request.Headers)
            {
                parameters.Headers.Set(pair.Key, pair.Value[0]);
                parameters.Set(pair.Key, pair.Value[0]);
            }

            return parameters;
        }

        public static string ExtractFromQuery(string parameter, HttpRequest request)
        {
            return request.Query.TryGetValue(parameter, out StringValues sortValues)
                ? sortValues.ToString()
                : string.Empty;
        }

        public static SortParams GetSortParams(HttpRequest request)
        {
            var sort = new SortParams();
            var parser = FilterParams.FromString(ExtractFromQuery("sort", request));

            foreach (var sortParam in parser)
            {
                sort.Add(new SortField(sortParam.Key, Convert.ToBoolean(sortParam.Value)));
            }

            return sort;
        }
    }
}
