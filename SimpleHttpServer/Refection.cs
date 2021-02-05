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
    public static class Reflection
    {
        public static Dictionary<string, Type> FunctionRegisted { get; private set; } = new Dictionary<string, Type>();

        public static HashSet<RouteInfo> RouteRegistered { get; private set; } = new HashSet<RouteInfo>();
        //public static Dictionary<string, RouteInfo> RouteRegistered { get; private set; } = new Dictionary<string, RouteInfo>();

        public static void RegisterFunctions()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            List<Type> listFunctions =
                assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(BaseFunction))).ToList();

            foreach (Type type in listFunctions)
            {
                string key = Regex.Replace(type.Name, @"(Function)\z", string.Empty);
                FunctionRegisted.Add(key, type);
                //LogController.LogInfo(string.Format(ResourceEnum.FunctionLoaded, name));
            }
        }

        public static void Execute(RequestModel request, out ResponseModel response)
        {
            response = new ResponseModel();
            if (FunctionRegisted.ContainsKey(request.Function))
            {
                BaseFunction function =
                    (BaseFunction)Activator.CreateInstance(FunctionRegisted[request.Function]);
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
                    string[] baseRoutes = type.GetCustomAttributes<RouteBaseAttribute>()
                        .ToList()
                        .Where(x => !string.IsNullOrWhiteSpace(x.UrlBase))
                        .Select(x => x.UrlBase)
                        .ToArray();
                    if (baseRoutes.Length == 0)
                    {
                        baseRoutes = new string[] { typeName };
                    }

                    foreach (string baseRoute in baseRoutes)
                    {
                        foreach (MethodInfo method in methodInfos)
                        {
                            RouteAttribute[] routeAttributes =
                                method.GetCustomAttributes<RouteAttribute>().ToArray();

                            RouteInfo routeInfo = null;
                            if (routeAttributes.Length == 0)
                            {
                                RouteActioner.BuildRouter(baseRoute, method, ref routeInfo);
                                RouteRegistered.Add(routeInfo);
                                continue;
                            }
                            foreach (RouteAttribute routeAttribute in routeAttributes)
                            {
                                //case: start with '/'
                                if (routeAttribute.Route.StartsWith('/'))
                                {
                                    RouteActioner.BuildRouter(string.Empty, method, ref routeInfo, routeAttribute);
                                    RouteRegistered.Add(routeInfo);
                                    continue;
                                }

                                RouteActioner.BuildRouter(baseRoute, method, ref routeInfo, routeAttribute);
                                RouteRegistered.Add(routeInfo);
                            }
                        }
                    }

                }
            }
        }


    }
}
