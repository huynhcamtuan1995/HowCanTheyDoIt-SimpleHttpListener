using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
                var index = method.GetParameters()
                    .ToList()
                    .FindIndex(x => x.Name.ToLower() == split[0].ToLower());
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
    }
}
