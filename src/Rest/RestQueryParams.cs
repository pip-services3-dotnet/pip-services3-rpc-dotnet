//using System.Collections.Generic;
//using PipServices.Commons.Convert;
//using PipServices.Commons.Data;

//namespace PipServices.Rpc.Services
//{
//    public sealed class RestQueryParams : MultiValueDictionary<string, object>
//    {
//        public RestQueryParams() { }

//        public RestQueryParams(string correlationId)
//        {
//            AddCorrelationId(correlationId);
//        }

//        public RestQueryParams(string correlationId, FilterParams filter, PagingParams paging)
//        {
//            AddCorrelationId(correlationId);
//            AddFilterParams(filter);
//            AddPagingParams(paging);
//        }

//        public void AddCorrelationId(string correlationId)
//        {
//            if (string.IsNullOrWhiteSpace(correlationId))
//                return;

//            Add("correlation_id", correlationId);
//        }

//        public void AddFilterParams(FilterParams filter)
//        {
//            if (filter == null) return;

//            foreach(var entry in filter)
//            {
//                var value = entry.Value;

//                if (value != null)
//                    Add(entry.Key, value);
//            }
//        }

//        public void AddPagingParams(PagingParams paging)
//        {
//            if (paging == null) return;

//            if (paging.Skip != null)
//                Add("skip", StringConverter.ToString(paging.Skip));

//            if (paging.Take != null)
//                Add("take", StringConverter.ToString(paging.Take));

//            Add("total", StringConverter.ToString(paging.Total));
//        }
//    }
//}
