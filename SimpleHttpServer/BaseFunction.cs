using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;

namespace SimpleHttpServer
{
    public abstract class BaseFunction
    {
        public const string Name = "BaseFunction";

        protected RequestModel _request;
        protected ResponseModel _response;
        public abstract void Execute();

        public void Initialize(RequestModel request)
        {
            _request = request;
        }

        public void GetResponse(out ResponseModel response)
        {
            response = _response;
            _response = null;
        }

    }

    public class TestAFunction : BaseFunction
    {
        public const string Name = "TestAFunction";

        public override void Execute()
        {
            RequestModel request = base._request;

            object data = JsonConvert.DeserializeObject<object>(request.Data);

            Console.WriteLine($"{Name}-{data.ToString()}");

            bool result = true;
            if (result)
            {
                _response = new ResponseModel
                {
                    Code = (int)HttpStatusCode.OK,
                    Description = nameof(HttpStatusCode.OK),
                    Data = JsonConvert.SerializeObject(data)
                };
            }
        }
    }

    public class TestBFunction : BaseFunction
    {
        public const string Name = "TestBFunction";

        public override void Execute()
        {
            RequestModel request = base._request;

            object data = JsonConvert.DeserializeObject<object>(request.Data);

            Console.WriteLine($"{Name}-{data.ToString()}");

            bool result = true;
            if (result)
            {
                _response = new ResponseModel
                {
                    Code = (int)HttpStatusCode.OK,
                    Description = $"{nameof(HttpStatusCode.OK)} - Base64 data",
                    Data = JsonConvert.SerializeObject(data).Base64Encode()
                };
            }
        }
    }



}
