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
    public class HandleRequestV2 : HandleBase
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
                .SelectMany(x => x.GetCustomAttributes<RouteAttribute>())
                .ToList()
                .Where(y => y.Route.StartsWith('/'))
                .ToArray();

            //have absolute routes, etc: /test/this/is/route/method ...
            if (absoluteRoutes.Length > 0)
            {
                RouteAttribute route = absoluteRoutes.FirstOrDefault(x => x.Route.TrimEnd('/') == url);
                if (route != null)
                {
                    MethodInfo method = methods.ToList().Find(x => x.GetCustomAttribute<RouteAttribute>()?.Route == route.Route);

                    string[] queries = request.Url.Query.TrimStart('?')
                        .Split('&', StringSplitOptions.RemoveEmptyEntries);

                    object[] parametters = new object[method.GetParameters().Length];
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
                            MethodInfo method = methods.ToList().Find(x => x.GetCustomAttribute<RouteAttribute>()?.Route == routeAttr.Route);

                            string[] parameterInfos = method.GetParameters().Select(x => x.Name).ToArray();
                            object[] parametters = new object[parameterInfos.Length];
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
                ((type.GetCustomAttribute<RouteBaseAttribute>() != null &&
                 url.StartsWith(type.GetCustomAttribute<RouteBaseAttribute>().UrlBase)) ||
                 url.StartsWith(Regex.Replace(type.Name, @"(Action)\z", string.Empty))));

            if (type == null)
            {
                //not found type class
                return;
            }
            string matchBaseRoute = type.GetCustomAttributes<RouteBaseAttribute>().Count() > 0
                ? (type.GetCustomAttributes<RouteBaseAttribute>()
                    .First(x => url.StartsWith(x.UrlBase))).UrlBase
                : Regex.Replace(type.Name, @"(Action)\z", string.Empty);

            string restPartRoute = Regex.Replace(url, $@"^{matchBaseRoute}", string.Empty).TrimStart('/');

            methods = type.GetMethods().Where(x => x.DeclaringType.Name == type.Name).ToArray();

            foreach (var method in methods)
            {
                //found absolute route
                string[] routeNames = method.GetCustomAttributes<RouteAttribute>()
                    .Where(x => !x.Route.StartsWith('/') &&
                        !string.IsNullOrWhiteSpace(x.Route))
                    .Select(x => x.Route)
                    .ToArray();
                if (routeNames.Length == 0)
                {
                    routeNames.Append(method.Name);
                }
                foreach (var route in routeNames)
                {
                    MatchCollection matches = Regex.Matches(route,
                        @"(?<={)(?<name>[a-zA-Z0-9_-]+)(?=})",
                        RegexOptions.IgnoreCase | RegexOptions.Multiline);

                    string absoluteUrl = $"{route}";
                    if (matches.Count > 0)
                    {
                        absoluteUrl = $"{Regex.Replace(absoluteUrl, @"{(?<name>[a-zA-Z0-9_-]+)}", @"([a-zA-Z0-9_-]+)")}";
                        ////var data = Regex.Matches("/api/param1/param2", absoluteUrl);

                        //var param = method.GetParameters()
                        //    .Select(x => x.Name)
                        //    .ToArray();
                        //routeInfo.ParamSegments = segmentUrl.Select(x => Array.FindIndex(param, p => $"{{{p}}}" == x)).ToArray();

                    }
                    if (Regex.IsMatch(restPartRoute, absoluteUrl))
                    {

                    }

                }
            }

            //out foreach mean not any match

            return;
        }
    }
}
