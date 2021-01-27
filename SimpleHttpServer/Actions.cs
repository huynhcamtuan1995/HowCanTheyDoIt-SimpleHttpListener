using System;

namespace SimpleHttpServer
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class RouteBaseAttribute : Attribute
    {

        public RouteBaseAttribute(string urlBase)
        {
            this.UrlBase = urlBase.TrimStart('/');
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

    
    class ATestAction : IRouteHandler
    {
        [Route(httpVerb: HttpMethod.POST)]
        public object MethodA()
        {
            return new object();
        }
    }

    class BTestAction : IRouteHandler
    {
        [Route(httpVerb: HttpMethod.POST, route: "B/test")]
        public object MethodB()
        {
            return new object();
        }

        public object MethodBB()
        {
            return new object();
        }
    }

    [RouteBase("Test")]
    class CTestAction : IRouteHandler
    {
        public object MethodC()
        {
            return new object();
        }
    }
}
