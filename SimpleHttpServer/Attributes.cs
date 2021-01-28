using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleHttpServer
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class RouteBaseAttribute : Attribute
    {

        public RouteBaseAttribute(string urlBase)
        {
            UrlBase = urlBase.TrimStart('/');
        }

        public string UrlBase { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class RouteAttribute : Attribute

    {

        public RouteAttribute(string route = "", HttpMethod httpVerb = HttpMethod.GET)
        {
            Route = route;
            HttpVerb = httpVerb;
        }

        public string Route { get; private set; }
        public HttpMethod HttpVerb { get; private set; }
    }
}
