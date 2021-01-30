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

        public static bool IsExistParametter(string[] segmentQuery, string paramName, out object paramValue)
        {
            paramValue = null;
            foreach (var query in segmentQuery)
            {
                var split = query.Split('=', 2);
                if (split[0].ToLower() == paramName.ToLower())
                {
                    paramValue = split[1];
                    return true;
                }
                return false;
            }
            return false;
        }

        public static void BuildRouter(string baseRoute, MethodInfo method, out RouteInfo routeInfo, RouteAttribute attribute = null)
        {
            routeInfo = new RouteInfo();
            string restPart = attribute != null && !string.IsNullOrWhiteSpace(attribute.Route)
                ? attribute.Route
                : method.Name;

            string absoluteUrl = Regex.Replace($"/{baseRoute}/{restPart}", "(/)+", "/");
            string[] segmentUrl = absoluteUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);

            MatchCollection matches = Regex.Matches(absoluteUrl,
                @"(?<={)(?<name>[a-zA-Z0-9_-]+)(?=})",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);

            if (matches.Count > 0)
            {
                absoluteUrl = $"{Regex.Replace(absoluteUrl, @"{(?<name>[a-zA-Z0-9_-]+)}", @"([a-zA-Z0-9_-]+)")}";
                //var data = Regex.Matches("/api/param1/param2", absoluteUrl);

                var param = method.GetParameters()
                    .Select(x => x.Name)
                    .ToArray();
                routeInfo.ParamSegments = segmentUrl.Select(x => Array.FindIndex(param, p => $"{{{p}}}" == x)).ToArray();
            }

            routeInfo.AbsoluteUrl = $"^{absoluteUrl}$";
            routeInfo.Action = method.DeclaringType;
            routeInfo.Method = method;
            routeInfo.ParamNames = method.GetParameters().Select(x => x.Name.ToLower()).ToArray();
            routeInfo.HttpVers = attribute != null
                ? attribute.HttpVerb
                : HttpMethod.GET;
        }

    }
}
