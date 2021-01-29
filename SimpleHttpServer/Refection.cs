using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleHttpServer
{
    public static class Reflection
    {
        public static Dictionary<string, Type> FunctionDictionary { get; private set; } = new Dictionary<string, Type>();
        public static Dictionary<string, RouteInfo> RouteDictionary { get; private set; } = new Dictionary<string, RouteInfo>();
        public static void RegisterFunctions()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            List<Type> listFunctions =
                assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(BaseFunction))).ToList();

            foreach (Type type in listFunctions)
            {
                string key = Regex.Replace(type.Name, @"(Function)\z", string.Empty);
                FunctionDictionary.Add(key, type);
                //LogController.LogInfo(string.Format(ResourceEnum.FunctionLoaded, name));
            }
        }

        public static void Execute(RequestModel request, out ResponseModel response)
        {
            response = new ResponseModel();
            if (FunctionDictionary.ContainsKey(request.Function))
            {
                BaseFunction function =
                    (BaseFunction)Activator.CreateInstance(FunctionDictionary[request.Function]);
                function.Initialize(request);
                function.Execute();
                function.GetResponse(out response);
            }
        }

        public static void RegisterRoutes()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            List<Type> types = assembly.GetTypes()
                .Where(type => type.GetInterfaces()
                    .Contains(typeof(IRouteHandler)))
                .ToList();

            foreach (Type type in types)
            {
                if (!RouteActioner.IsEmptyMethodClass(type, out MethodInfo[] methodInfos))
                {
                    string typeName = Regex.Replace(type.Name, @"(Action)\z", string.Empty);
                    string[] baseRoutes = (string[])((RouteBaseAttribute[])type
                        .GetCustomAttributes(typeof(RouteBaseAttribute)))
                        .ToList()
                        .Where(x => !string.IsNullOrWhiteSpace(x.UrlBase))
                        .Select(x => x.UrlBase)
                        .ToArray();
                    if (baseRoutes.Count() == 0)
                    {
                        baseRoutes.Append(typeName);
                    }

                    foreach (string baseRoute in baseRoutes)
                    {
                        foreach (MethodInfo method in methodInfos)
                        {
                            RouteAttribute[] routeAttributes =
                                (RouteAttribute[])method.GetCustomAttributes(typeof(RouteAttribute));

                            if (routeAttributes.Count() == 0)
                            {
                                RouteActioner.BuildRouter(baseRoute, method, out RouteInfo routeInfo);
                                RouteDictionary.Add(routeInfo.AbsoluteUrl, routeInfo);
                            }
                            foreach (RouteAttribute routeAttribute in routeAttributes)
                            {
                                RouteInfo routeInfo = new RouteInfo();
                                //case: start with '/'
                                if (routeAttribute.Route.StartsWith('/'))
                                {
                                    RouteActioner.BuildRouter(string.Empty, method, out routeInfo, routeAttribute);
                                    RouteDictionary.Add(routeInfo.AbsoluteUrl, routeInfo);
                                    continue;
                                }

                                RouteActioner.BuildRouter(baseRoute, method, out routeInfo, routeAttribute);
                                RouteDictionary.Add(routeInfo.AbsoluteUrl, routeInfo);
                            }
                        }
                    }

                }
            }
        }


    }
}
