using System;
using System.Collections.Generic;
using System.Text;

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
}
