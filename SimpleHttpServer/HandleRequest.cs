using System;
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
    public class HandleRequest : HandleBase
    {
        public static async Task ExecuteRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            RouteInfo routeInfo = Reflection.RouteRegistered
                .FirstOrDefault(x => Regex.IsMatch(request.Url.AbsolutePath,
                    Regex.Replace(x.AbsoluteUrl, @"(?<=\$).+", string.Empty)) &&
                        x.HttpVers.ToString() == request.HttpMethod);

            try
            {
                if (routeInfo != null &&
                    request.HttpMethod == routeInfo.HttpVers.ToString())
                {
                    ExecuteRoute(routeInfo, request, response);
                }
            }
            catch (Exception ex)
            {
                OutputResponse(ex, request, response);
            }
            return;
        }

        private static HttpListenerResponse ExecuteRoute(RouteInfo routeInfo, HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                object[] parametters = new object[routeInfo.ParamNames.Length];

                ExtractParamSegment(request, routeInfo, ref parametters);

                if (routeInfo.HttpVers == HttpMethod.POST ||
                    routeInfo.HttpVers == HttpMethod.PUT)
                {
                    ExtractParamBody(request, routeInfo, ref parametters);
                }

                ExtractParamQuery(request, routeInfo, ref parametters);

                object actionValue = ExecuteMethod(routeInfo.Method, parametters);
                OutputResponse(actionValue, request, response);
            }
            catch (Exception ex)
            {
                OutputResponse(ex, request, response);
            }

            return response;
        }
    }
}
