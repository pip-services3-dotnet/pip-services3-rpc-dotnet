using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PipServices.Commons.Convert;
using PipServices.Commons.Errors;

namespace PipServices.Rpc.Services
{
    public static class HttpResponseSender
    {
        public static async Task SendErrorAsync(HttpResponse response, Exception ex)
        {
            // Unwrap exception
            if (ex is AggregateException)
            {
                var ex2 = ex as AggregateException;
                ex = ex2.InnerExceptions.Count > 0 ? ex2.InnerExceptions[0] : ex;
            }

            if (ex is PipServices.Commons.Errors.ApplicationException)
            {
                response.ContentType = "application/json";
                var ex3 = ex as PipServices.Commons.Errors.ApplicationException;
                response.StatusCode = ex3.Status;
                var contentResult = JsonConverter.ToJson(ErrorDescriptionFactory.Create(ex3));
                await response.WriteAsync(contentResult);
            }
            else
            {
                response.ContentType = "application/json";
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var contentResult = JsonConverter.ToJson(ErrorDescriptionFactory.Create(ex));
                await response.WriteAsync(contentResult);
            }
        }

        public static async Task SendResultAsync(HttpResponse response, object result)
        {
            if (result == null)
            {
                response.StatusCode = (int)HttpStatusCode.NoContent;
            }
            else
            {
                response.ContentType = "application/json";
                response.StatusCode = (int)HttpStatusCode.OK;
                var contentResult = JsonConverter.ToJson(result);
                await response.WriteAsync(contentResult);
            }
        }

        public static async Task SendEmptyResultAsync(HttpResponse response)
        {
            response.ContentType = "application/json";
            response.StatusCode = (int)HttpStatusCode.NoContent;
            await Task.Delay(0);
        }

        public static async Task SendCreatedResultAsync(HttpResponse response, object result)
        {
            if (result == null)
            {
                response.StatusCode = (int)HttpStatusCode.NoContent;
            }
            else
            {
                response.ContentType = "application/json";
                response.StatusCode = (int)HttpStatusCode.Created;
                var contentResult = JsonConverter.ToJson(result);
                await response.WriteAsync(contentResult);
            }
        }

        public static async Task SendDeletedResultAsync(HttpResponse response, object result)
        {
            if (result == null)
            {
                response.StatusCode = (int)HttpStatusCode.NoContent;
            }
            else
            {
                response.ContentType = "application/json";
                response.StatusCode = (int)HttpStatusCode.OK;
                var contentResult = JsonConverter.ToJson(result);
                await response.WriteAsync(contentResult);
            }
        }

    }
}
