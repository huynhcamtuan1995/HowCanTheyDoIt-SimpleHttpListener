using System;
using System.Reflection;

namespace SimpleHttpServer
{
    public class RequestModel
    {
        public string Function { get; set; }
        public string Data { get; set; }
        public string HashString { get; set; }
    }
    public class ResponseModel
    {
        public int Code { get; set; }
        public string Description { get; set; }
        public string Data { get; set; }
        public string HashString { get; set; }
    }

    public class RouteResult
    {
        public object Handler { get; set; }
    }

    public class RouteInfo
    {
        public Type Action { get; set; }
        public MethodInfo Method { get; set; }
        public HttpMethod HttpVers { get; set; }
        public int[] ParamSegments { get; set; } = new int[] { };
        public string[] ParamNames { get; set; }
        public string AbsoluteUrl { get; set; }
        public string BodyParametter { get; set; }
    }

    public class RequestInfo
    {
        public string test1 { get; set; }
        public string test2 { get; set; }
    }
}
