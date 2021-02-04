using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpServer
{
    public class HandleBase
    {
        protected static bool OutputResponse(object actionValue, HttpListenerRequest request, HttpListenerResponse response)
        {
            ResponseModel responseModel = new ResponseModel();
            if(actionValue == null)
            {
                responseModel.Code = (int)HttpStatusCode.BadRequest;
                responseModel.Description = $"{nameof(HttpStatusCode.BadRequest)}";
                responseModel.Data = JsonConvert.SerializeObject(actionValue);
            }

            responseModel.Code = (int)HttpStatusCode.OK;
            responseModel.Description = $"{nameof(HttpStatusCode.OK)}";
            responseModel.Data = JsonConvert.SerializeObject(actionValue);

            //responseModel.Code = (int)HttpStatusCode.NotFound;
            //responseModel.Description = $"{nameof(HttpStatusCode.NotFound)}";
            //responseModel.Data = JsonConvert.SerializeObject(actionValue);

            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseModel));
            response.ContentType = request.ContentType;
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = data.LongLength;

            response.OutputStream.Write(data, 0, data.Length);

            return true;
        }

        protected static object ExecuteMethod(MethodInfo method, params object[] parametters)
        {
            Type actionType = method.DeclaringType;
            ConstructorInfo actionConstructor = actionType.GetConstructor(Type.EmptyTypes);
            object actionClassObject = actionConstructor.Invoke(new object[] { });

            object actionValue = method.Invoke(actionClassObject, parametters);
            return actionValue;
        }

        protected static object[] ObtainParametters(MethodInfo method, string[] queries, object[] parametters)
        {
            foreach (var query in queries)
            {
                var split = query.Split('=', 2);

                var index = Array.FindIndex(
                    method.GetParameters(), 
                    x => x.Name.ToLower() == split[0].ToLower());

                if (index >= 0)
                {
                    parametters[index] = split[1];
                }
            }
            return parametters;
        }

        protected static bool IsExistParametter(string[] segmentQuery, string paramName, out object paramValue)
        {
            paramValue = null;
            foreach (var query in segmentQuery)
            {
                var split = query.Split('=', 2);
                if (split[0].ToLower() == paramName.ToLower())
                {
                    paramValue = split[1];
                    return true;
                }
                return false;
            }
            return false;
        }

        protected static void ExtractParamSegment(HttpListenerRequest request, RouteInfo routeInfo, ref object[] parametters)
        {
            string[] segmentUrl = request.Url.AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            int index = 0;
            foreach (var param in routeInfo.ParamSegments)
            {
                if (param >= 0)
                {
                    parametters[param] = segmentUrl[index];
                }
                index++;
            }
        }

        protected static void ExtractParamQuery(HttpListenerRequest request, RouteInfo routeInfo, ref object[] parametters)
        {
            string[] segmentQuery = request.Url.Query.TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries);
            if (segmentQuery.Length > 0)
            {
                for (int i = 0; i < parametters.Length; i++)
                {
                    if (parametters[i] == null &&
                        IsExistParametter(segmentQuery, routeInfo.ParamNames[i], out object queryValue))
                    {
                        parametters[i] = queryValue;
                    }
                }
            }
        }

        protected static void ExtractParamBody(HttpListenerRequest request, RouteInfo routeInfo, ref object[] parametters)
        {
            if (routeInfo.BodyParametter != null)
            {
                Stream stream = request.InputStream;
                Encoding encoding = request.ContentEncoding;
                StreamReader reader = new StreamReader(stream, encoding);
                string body = reader.ReadToEnd();

                var value = JsonConvert.DeserializeObject(body, Type.GetType(routeInfo.BodyParametter));
                int i = Array.FindIndex(
                    routeInfo.Method.GetParameters(),
                    x => x.ParameterType.FullName == routeInfo.BodyParametter);
                parametters[i] = value;
            }
        }
    }
}
