using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

            //have absolute routes, etc: /test/this/is/route/method ...
            if (ExecuteAbsoluteMethod(types, request, response))
            {
                return;
            }

            //rest of route and basic method name(non-attribute) routes here, etc: test/this/is/route or non-attribute
            if (ExecuteRelativeMethod(types, request, response))
            {
                return;
            }

            //out foreach mean not any match
            return;
        }


        private static bool ExecuteAbsoluteMethod(List<Type> types, HttpListenerRequest request, HttpListenerResponse response)
        {
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
                    return true;
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
                                int index = Array.FindIndex(parameterInfos, x => $"{{{x}}}" == param);
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
                            return true;
                        }
                    }
                }
            }

            return false;
        }


        private static bool ExecuteRelativeMethod(List<Type> types, HttpListenerRequest request, HttpListenerResponse response)
        {
            string url = request.Url.AbsolutePath.TrimStart('/');

            Type type = types.Find(type =>
              ((type.GetCustomAttribute<RouteBaseAttribute>() != null &&
               url.StartsWith(type.GetCustomAttribute<RouteBaseAttribute>().UrlBase)) ||
               url.StartsWith(Regex.Replace(type.Name, @"(Action)\z", string.Empty))));

            if (type == null)
            {
                //not found type class
                return false;
            }
            string matchBaseRoute = type.GetCustomAttributes<RouteBaseAttribute>().Count() > 0
                ? (type.GetCustomAttributes<RouteBaseAttribute>()
                    .First(x => url.StartsWith(x.UrlBase))).UrlBase
                : Regex.Replace(type.Name, @"(Action)\z", string.Empty);

            string restPartRoute = Regex.Replace(url, $@"^{matchBaseRoute}", string.Empty).TrimStart('/');

            MethodInfo[] methods = type.GetMethods().Where(x => x.DeclaringType.Name == type.Name).ToArray();

            foreach (var method in methods)
            {
                //found absolute route
                string[] routeNames = method.GetCustomAttributes<RouteAttribute>()
                    .Where(x => !x.Route.StartsWith('/') &&
                        !string.IsNullOrWhiteSpace(x.Route) &&
                        request.HttpMethod == x.HttpVerb.ToString())
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

                    string buildUrl = $"{route}";
                    if (matches.Count > 0)
                    {
                        buildUrl = $"{Regex.Replace(buildUrl, @"{(?<name>[a-zA-Z0-9_-]+)}", @"([a-zA-Z0-9_-]+)")}";
                    }
                    if (Regex.IsMatch(restPartRoute, buildUrl, RegexOptions.IgnoreCase))
                    {
                        //url segments
                        string[] segmentUrl = restPartRoute.Split('/', StringSplitOptions.RemoveEmptyEntries);

                        //router segments
                        string[] segmentRoute = route.Split('/', StringSplitOptions.RemoveEmptyEntries);

                        object[] parametters = new object[method.GetParameters().Length];

                        ParameterInfo[] methodParametters = method.GetParameters();

                        int index = 0;
                        foreach (var param in segmentRoute)
                        {
                            int i = Array.FindIndex(methodParametters, x => param == $"{{{x.Name}}}");
                            if (i >= 0)
                            {
                                parametters[i] = segmentUrl[index];
                            }
                            index++;
                        }

                        var bodyParametter = methodParametters.FirstOrDefault(x => x.ParameterType != typeof(string))?.ParameterType.FullName;

                        if (!string.IsNullOrWhiteSpace(bodyParametter))
                        {
                            Stream stream = request.InputStream;
                            Encoding encoding = request.ContentEncoding;
                            StreamReader reader = new StreamReader(stream, encoding);
                            string body = reader.ReadToEnd();

                            var value = JsonConvert.DeserializeObject(body, Type.GetType(bodyParametter));
                            int i = Array.FindIndex(
                                methodParametters,
                                x => x.ParameterType.FullName == bodyParametter);
                            parametters[i] = value;
                        }

                        string[] segmentQuery = request.Url.Query.TrimStart('?')
                            .Split('&', StringSplitOptions.RemoveEmptyEntries);
                        if (segmentQuery.Length > 0)
                        {
                            for (int i = 0; i < parametters.Length; i++)
                            {
                                if (parametters[i] == null &&
                                    IsExistParametter(segmentQuery, segmentRoute[i], out object queryValue))
                                {
                                    parametters[i] = queryValue;
                                }
                            }
                        }

                        object actionValue = ExecuteMethod(method, parametters);
                        OutputResponse(actionValue, request, response);
                    }
                }
            }
            return true;
        }
    }
}
