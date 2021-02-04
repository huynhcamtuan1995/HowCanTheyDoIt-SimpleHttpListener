using System;
using System.Collections.Generic;

namespace SimpleHttpServer
{
    [RouteBase("test")]
    class ATestAction : IRouteHandler
    {
        [Route(httpVerb: HttpMethod.POST)]
        public object MethodA()
        {
            return new object();
        }

        public object MethodAA()
        {
            return new object();
        }
        [Route(route: "abc/method")]
        public object MethodAAA(string abc, string t4em)
        {
            return new string[] { abc, t4em };
        }

        [Route(route: "/api/{yyy}/{a}/{abc}")]
        public object MethodAAAA(string abc, string t4em)
        {
            return new string[] { abc, t4em };
        }

        [Route(route: "api/{param1}/{param2}", HttpMethod.POST)]
        public object MethodAAAAA(string param1, RequestInfo request)
        {
            return new List<object>(){ param1, request };
        }
    }

    //class BTestAction : IRouteHandler
    //{
    //    [Route(httpVerb: HttpMethod.POST, route: "B/test")]
    //    public object MethodB()
    //    {
    //        return new object();
    //    }

    //    public object MethodBB()
    //    {
    //        return new object();
    //    }
    //}

    //[RouteBase("Test")]
    //class CTestAction : IRouteHandler
    //{
    //    public object MethodC()
    //    {
    //        return new object();
    //    }
    //}
}
