using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleHttpServer
{


    public interface IRouteHandler
    {
    }


    public static class RouteActioner
    {
        public static bool IsEmptyMethodClass(Type type, out MethodInfo[] methodInfos)
        {
            methodInfos = (MethodInfo[])type.GetMethods().Where(x => x.DeclaringType.Name == type.Name).ToArray();
            if (methodInfos.Count() == 0)
            {
                return true;
            }
            return false;
        }

        public static void BuildRouter(string baseRoute, MethodInfo method, out RouteInfo routeInfo, RouteAttribute attribute = null)
        {
            routeInfo = new RouteInfo();
            string restPart = attribute != null && !string.IsNullOrWhiteSpace(attribute.Route)
                ? attribute.Route
                : method.Name;



            string absoluteUrl = $"/{baseRoute}/{restPart}".Replace("//", "/");
            routeInfo.AbsoluteUrl = absoluteUrl;
            routeInfo.Segments = absoluteUrl.Split('/');
            routeInfo.Action = method.DeclaringType;
            routeInfo.Method = method;
            routeInfo.HttpVers = attribute != null
                ? attribute.HttpVerb
                : HttpMethod.GET;
            routeInfo.ParameterInfos = method.GetParameters();

        }

    }

    public class MethodActioner
    {
        public bool IsUrlMatch(string baseRoute, string requestUrl, string httpMethod)
        {
            bool result = false;
            string restToken = baseRoute.Replace(@"/", "");
            string pattern = string.Format("^(\\/{0}\\/)([1-9]+[0-9]*)$", restToken);
            Regex regEx = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match m = regEx.Match(requestUrl);


            if (httpMethod == "POST")
            {
                result = requestUrl == baseRoute;
            }

            if (httpMethod == "GET")
            {
                bool validRoute1 = requestUrl == baseRoute;
                bool validRoute2 = m.Success;
                result = validRoute1 || validRoute2;
            }

            if (httpMethod == "PUT" || httpMethod == "DELETE")
            {
                bool validRoute1 = requestUrl == baseRoute;
                bool validRoute2 = m.Success;
                result = validRoute1 || validRoute2;
            }

            return result;
        }

        public async Task<RouteResult> FindHandler(Type handlerTypeRequired,
            HttpListenerContext context, IList<Type> handlers)
        {

            return await Task.Run(async () =>
            {
                var httpMethod = context.Request.HttpMethod;
                var url = context.Request.RawUrl.TrimStart('/');
                RouteResult result = new RouteResult();
                foreach (var handler in handlers)
                {
                    if (handler.GetInterfaces().Any(x => x.Name == handlerTypeRequired.Name))
                    {
                        var routeBase = ((RouteBaseAttribute[])handler
                            .GetCustomAttributes(typeof(RouteBaseAttribute)))
                            .ToList();

                        bool isBaseMatch = routeBase.Find(x =>
                            url.StartsWith(x.UrlBase, StringComparison.CurrentCultureIgnoreCase)) != null ||
                            url.StartsWith(Regex.Replace(handler.Name, @"(Action)\z", string.Empty),
                                StringComparison.CurrentCultureIgnoreCase);

                    }
                }
                return result;
            });

        }
    }
}
