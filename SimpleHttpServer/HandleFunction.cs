using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpServer
{
    public static class HandleFunction
    {
        public static async Task ExecuteFunction(HttpListenerRequest request, HttpListenerResponse response)
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
    }
}
