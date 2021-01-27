using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SimpleHttpServer
{
    public static class ParseContext
    {
        public static async Task WriteData(HttpListenerRequest req, HttpListenerResponse resp)
        {
            try
            {
                RequestModel request = new RequestModel();
                switch (req.HttpMethod)
                {
                    case "GET":

                        break;
                    case "POST":
                        using (Stream body = req.InputStream) // here we have data
                        using (StreamReader reader = new StreamReader(body, req.ContentEncoding))
                        {
                            string json = reader.ReadToEnd();
                            request = JsonConvert.DeserializeObject<RequestModel>(json);
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

                Refection.Execute(request, out ResponseModel response);

                byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                resp.ContentType = req.ContentType;
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                await resp.OutputStream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
