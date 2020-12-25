using System.Collections.Specialized;
using System.Text;

using PipServices3.Commons.Convert;
using PipServices3.Commons.Data;

namespace PipServices3.Rpc.Clients
{
    /// <summary>
    /// Route builder helper class to create route based on input parameters.
    /// 
    /// It might be useful in rest clients for microservices built on top of rest operations.
    /// </summary>
    /// <example>
    /// <code>
    /// var route = RouteBuilder
    ///     .Route("get_dummies")
    ///     .AddFilterParams(filter)
    ///     .AddPagingParams(paging)
    ///     .AddSortParams(sort)
    ///     .Build()
    /// </code>
    /// </example>
    public class RouteBuilder
    {
        private string _route;
        NameValueCollection _queryParameters = new NameValueCollection();

        private RouteBuilder(string route)
        {
            _route = route;
        }

        public static RouteBuilder Route(string route)
        {
            return new RouteBuilder(route);
        }

        public RouteBuilder AddParameter(string name, string value)
        {
            _queryParameters[name] = value;

            return this;
        }

        public RouteBuilder AddFilterParams(FilterParams filter)
        {
            if (filter == null)
            {
                return this;
            }

            _queryParameters["filter"] = filter.ToString();

            return this;
        }

        public RouteBuilder AddPagingParams(PagingParams paging)
        {
            if (paging == null)
            {
                return this;
            }

            if (paging.Skip.HasValue)
            {
                _queryParameters["skip"] = paging.Skip.Value.ToString();
            }

            if (paging.Take.HasValue)
            {
                _queryParameters["take"] = paging.Take.Value.ToString();
            }

            if (paging.Total)
            {
                _queryParameters["total"] = StringConverter.ToString(paging.Total);
            }

            return this;
        }

        public RouteBuilder AddSortParams(SortParams sort)
        {
            if (sort == null)
            {
                return this;
            }

            var map = new StringValueMap();

            foreach (var sortField in sort)
            {
                map[sortField.Name] = StringConverter.ToString(sortField.Ascending);
            }

            _queryParameters["sort"] = map.ToString();

            return this;
        }

        public RouteBuilder AddProjectionParams(ProjectionParams projection)
        {
            if (projection == null)
            {
                return this;
            }

            _queryParameters["projection"] = projection.ToString();

            return this;
        }

        public string Build()
        {
            var queryBuilder = new StringBuilder();

            foreach (string name in _queryParameters)
            {
                if (queryBuilder.Length > 0)
                {
                    queryBuilder.Append('&');
                }

                queryBuilder.Append(name);
                queryBuilder.Append('=');
                queryBuilder.Append(System.Web.HttpUtility.UrlEncode(_queryParameters[name]));
            }

            return _queryParameters.Count > 0 ? $"{_route}?{queryBuilder}" : _route;
        }
    }
}
