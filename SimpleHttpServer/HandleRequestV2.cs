using Newtonsoft.Json;
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
    public class HandleRequestV2 :  HandleBase
    {
        public static async Task ExecuteRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            List<Type> types = assembly.GetTypes()
                .Where(type => type.GetInterfaces()
                    .Contains(typeof(IRouteHandler)))
                .ToList();

            string[] segmentQuery = request.Url.Query.TrimStart('?')
                       .Split('&', StringSplitOptions.RemoveEmptyEntries);

            string url = request.Url.AbsolutePath.TrimEnd('/');
            MethodInfo[] methods = types.SelectMany(x => x.GetMethods()
                .Where(y => y.DeclaringType.Name == x.Name))
                .ToArray();
            RouteAttribute[] absoluteRoutes = methods
                .SelectMany(x => (RouteAttribute[])x.GetCustomAttributes(typeof(RouteAttribute)))
                .ToList()
                .Where(y => y.Route.StartsWith('/'))
                .ToArray();

            //have absolute routes, etc: /test/this/is/route/method ...
            if (absoluteRoutes.Count() > 0)
            {
                RouteAttribute route = absoluteRoutes.FirstOrDefault(x => x.Route.TrimEnd('/') == url);
                if (route != null)
                {
                    MethodInfo method = methods.ToList().Find(x => ((RouteAttribute)x.GetCustomAttribute(typeof(RouteAttribute)))?.Route == route.Route);

                    string[] queries = request.Url.Query.TrimStart('?')
                        .Split('&', StringSplitOptions.RemoveEmptyEntries);

                    object[] parametters = new object[method.GetParameters().Count()];
                    parametters = ObtainParametters(method, queries, parametters);
                    object actionValue = ExecuteMethod(method, parametters);

                    OutputResponse(actionValue, request, response);
                    return;
                }
                else
                {
                    foreach (RouteAttribute routeAttr in absoluteRoutes)
                    {
                        string attrUrl = routeAttr.Route;
                        string regexAbsolute = $"{Regex.Replace(attrUrl, @"{(?<name>[a-zA-Z0-9_-]+)}", @"([a-zA-Z0-9_-]+)")}";

                        Match match = Regex.Match(url, regexAbsolute, RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            MethodInfo method = methods.ToList().Find(x => ((RouteAttribute)x.GetCustomAttribute(typeof(RouteAttribute)))?.Route == routeAttr.Route);

                            string[] parameterInfos = method.GetParameters().Select(x => x.Name).ToArray();
                            object[] parametters = new object[parameterInfos.Count()];
                            string[] split = routeAttr.Route.Split('/', StringSplitOptions.RemoveEmptyEntries);
                            int i = 1;
                            foreach (var param in split)
                            {
                                int index = parameterInfos.ToList().FindIndex(x => $"{{{x}}}" == param);
                                if (index >= 0)
                                {
                                    parametters[index] = request.Url.Segments[i];
                                }
                                i++;
                            }

                            string[] queries = request.Url.Query.TrimStart('?')
                                .Split('&', StringSplitOptions.RemoveEmptyEntries);
                            parametters = ObtainParametters(method, queries, parametters);
                            object actionValue = ExecuteMethod(method, parametters);

                            OutputResponse(actionValue, request, response);
                            return;
                        }
                    }
                }
            }

            //rest of route and basic method name(non-attribute) routes here, etc: test/this/is/route or non-attribute
            url = request.Url.AbsolutePath.TrimStart('/');
            Type type = types.Find(type =>
                (((RouteBaseAttribute)type.GetCustomAttribute(typeof(RouteBaseAttribute)) != null &&
                 url.StartsWith(((RouteBaseAttribute)type.GetCustomAttribute(typeof(RouteBaseAttribute))).UrlBase)) ||
                 url.StartsWith(Regex.Replace(type.Name, @"(Action)\z", string.Empty))));

            if (type == null)
            {
                //not found type class
                return;
            }
            string matchBaseRoute = type.GetCustomAttributes(typeof(RouteBaseAttribute)).Count() > 0
                ? ((RouteBaseAttribute)type.GetCustomAttributes(typeof(RouteBaseAttribute))
                    .First(x => url.StartsWith(((RouteBaseAttribute)x).UrlBase))).UrlBase
                : Regex.Replace(type.Name, @"(Action)\z", string.Empty);

            string restPartRoute = Regex.Replace(url, $@"^{matchBaseRoute}", string.Empty);

            methods = type.GetMethods().Where(x => x.DeclaringType.Name == type.Name).ToArray();
            //MethodInfo methodInfo = methods.ToList().Find(x =>
            //    ((RouteAttribute)x.GetCustomAttribute(typeof(RouteAttribute)) != null &&
            //     ((RouteAttribute[])x.GetCustomAttributes(typeof(RouteAttribute)))
            //        .FirstOrDefault(y => y.Route.StartsWith('/') == false &&
            //            $"{matchBaseRoute}/{y.Route}" == url) != null));

            foreach (var method in methods)
            {
                //found absolute route
                if (method != null)
                {

                }
                else //maybe contain {params} route
                {

                }
            }
            string matchMethodRoute = string.Empty;


            return;
        }
    }
}
