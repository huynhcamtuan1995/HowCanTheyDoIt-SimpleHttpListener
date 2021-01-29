using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SimpleHttpServer
{
    public static class HandleContext
    {
        public static async Task ExecuteRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                RequestModel requestModel = new RequestModel();
                switch (request.HttpMethod)
                {
                    case "GET":

                        break;
                    case "POST":
                        using (Stream body = request.InputStream) // here we have data
                        using (StreamReader reader = new StreamReader(body, request.ContentEncoding))
                        {
                            string json = reader.ReadToEnd();
                            requestModel = JsonConvert.DeserializeObject<RequestModel>(json);
                        }
                        break;
                    case "PUT":
                        break;
                    case "DELETE":
                        break;
                    default:
                        break;

                }

                //Verify HashString
                //...
                Reflection.Execute(requestModel, out ResponseModel responseModel);

                byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseModel));
                response.ContentType = request.ContentType;
                response.ContentEncoding = Encoding.UTF8;
                response.ContentLength64 = data.LongLength;

                await response.OutputStream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {

            }
        }

        public static async Task ProgressRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            RouteInfo routeInfo = Reflection.RouteDictionary.FirstOrDefault(x => Regex.IsMatch(request.Url.AbsolutePath, x.Key)).Value;

            if (routeInfo != null && request.HttpMethod == routeInfo.HttpVers.ToString())
            {
                try
                {
                    //style: with out invoke contructor 
                    //object actionType = Activator.CreateInstance(routeInfo.Action);
                    Type actionType = routeInfo.Action;
                    ConstructorInfo actionConstructor = actionType.GetConstructor(Type.EmptyTypes);
                    object actionClassObject = actionConstructor.Invoke(new object[] { });

                    object[] parametters = new object[routeInfo.Method.GetParameters().Count()];
                    string[] segmentUrl = request.Url.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

                    int index = 0;
                    foreach (var param in routeInfo.ParamSegments)
                    {
                        if (param >= 0)
                        {
                            parametters[param] = segmentUrl[index];
                        }
                        ++index;
                    }

                    object actionValue = routeInfo.Method.Invoke(actionClassObject, parametters);
                    ResponseModel responseModel = new ResponseModel
                    {
                        Code = (int)HttpStatusCode.OK,
                        Description = $"{nameof(HttpStatusCode.OK)}",
                        Data = JsonConvert.SerializeObject(actionValue)
                    };

                    byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseModel));
                    response.ContentType = request.ContentType;
                    response.ContentEncoding = Encoding.UTF8;
                    response.ContentLength64 = data.LongLength;

                    await response.OutputStream.WriteAsync(data, 0, data.Length);
                }
                catch (Exception ex)
                {

                }
            }
        }


    }
}
