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
    public static class HandleContextV2
    {
        public static bool ExecuteRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            List<Type> types = assembly.GetTypes()
                .Where(type => type.GetInterfaces()
                    .Contains(typeof(IRouteHandler)))
                .ToList();

            string[] segmentQuery = request.Url.Query.TrimStart('?')
                       .Split('&', StringSplitOptions.RemoveEmptyEntries);

            string url = request.Url.AbsolutePath.TrimEnd('/');
            MethodInfo[] methods = types.SelectMany(x => x.GetMethods().Where(y => y.DeclaringType.Name == x.Name)).ToArray();
            RouteAttribute[] absoluteRoutes = methods
                .SelectMany(x => (RouteAttribute[])x.GetCustomAttributes(typeof(RouteAttribute)))
                .ToList()
                .Where(y => y.Route.StartsWith('/'))
                .ToArray();

            if (absoluteRoutes.Count() > 0)
            {
                RouteAttribute route = absoluteRoutes.FirstOrDefault(x => x.Route.TrimEnd('/') == url);
                if (route != null)
                {
                    MethodInfo method = methods.ToList().Find(x => ((RouteAttribute)x.GetCustomAttribute(typeof(RouteAttribute)))?.Route == route.Route);

                    string[] queries = request.Url.Query.TrimStart('?')
                        .Split('&', StringSplitOptions.RemoveEmptyEntries);

                    object[] parametters = new object[method.GetParameters().Count()];
                    parametters = GetParametters(method, queries, parametters);
                    object actionValue = ExecuteMethod(method, parametters);

                    GetResponseModel(actionValue, request, response);
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
                            parametters = GetParametters(method, queries, parametters);
                            object actionValue = ExecuteMethod(method, parametters);

                            GetResponseModel(actionValue, request, response);
                            return true;
                        }
                    }
                }
            }

            url = request.Url.AbsolutePath.TrimStart('/');
            Type type = types.Find(type => 
                (((RouteBaseAttribute)type.GetCustomAttribute(typeof(RouteBaseAttribute)) != null &&
                url.StartsWith(((RouteBaseAttribute)type.GetCustomAttribute(typeof(RouteBaseAttribute))).UrlBase)) ||
                url.StartsWith(Regex.Replace(type.Name, @"(Action)\z", string.Empty))));

            methods = type.GetMethods().Where(x => x.DeclaringType.Name == type.Name).ToArray();

            return true;
        }

        private static bool GetResponseModel(object actionValue, HttpListenerRequest request, HttpListenerResponse response)
        {
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

            response.OutputStream.Write(data, 0, data.Length);

            return true;
        }

        private static object[] GetParametters(MethodInfo method, string[] queries, object[] parametters)
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

        private static object ExecuteMethod(MethodInfo method, params object[] parametters)
        {
            Type actionType = method.DeclaringType;
            ConstructorInfo actionConstructor = actionType.GetConstructor(Type.EmptyTypes);
            object actionClassObject = actionConstructor.Invoke(new object[] { });

            object actionValue = method.Invoke(actionClassObject, parametters);
            return actionValue;
        }
    }
}
