using System;
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
            RouteInfo routeInfo = Reflection.RouteDictionary.FirstOrDefault(x => Regex.IsMatch(request.Url.AbsolutePath, x.Key)).Value;

            if (routeInfo != null && request.HttpMethod == routeInfo.HttpVers.ToString())
            {
                switch (routeInfo.HttpVers)
                {
                    case HttpMethod.GET:
                        ExecuteMethodGet(routeInfo, request, response);
                        return;
                    case HttpMethod.POST:
                        ExecuteMethodPost(routeInfo, request, response);
                        return;
                    case HttpMethod.PUT:
                        ExecuteMethodPut(routeInfo, request, response);
                        return;
                    case HttpMethod.DELETE:
                        ExecuteMethodDelete(routeInfo, request, response);
                        return;
                    default:
                        //return error
                        return;
                }
            }
        }

        private static HttpListenerResponse ExecuteMethodGet(RouteInfo routeInfo, HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                string[] segmentUrl = request.Url.AbsolutePath
                    .Split('/', StringSplitOptions.RemoveEmptyEntries);

                object[] parametters = new object[routeInfo.ParamNames.Count()];
                int index = 0;
                foreach (var param in routeInfo.ParamSegments)
                {
                    if (param >= 0)
                    {
                        parametters[param] = segmentUrl[index];
                    }
                    index++;
                }

                string[] segmentQuery = request.Url.Query.TrimStart('?')
                    .Split('&', StringSplitOptions.RemoveEmptyEntries);
                if (segmentQuery.Count() > 0)
                {
                    for (int i = 0; i < parametters.Count(); i++)
                    {
                        if (parametters[i] == null &&
                            IsExistParametter(segmentQuery, routeInfo.ParamNames[i], out object queryValue))
                        {
                            parametters[i] = queryValue;
                        }
                    }
                }

                object actionValue = ExecuteMethod(routeInfo.Method, parametters);
                OutputResponse(actionValue, request, response);

            }
            catch (Exception ex)
            {
                OutputResponse(ex, request, response);
            }

            return response;
        }

        private static HttpListenerResponse ExecuteMethodPost(RouteInfo routeInfo, HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                //style: with out invoke contructor 
                //object actionType = Activator.CreateInstance(routeInfo.Action);
                Type actionType = routeInfo.Action;
                ConstructorInfo actionConstructor = actionType.GetConstructor(Type.EmptyTypes);
                object actionClassObject = actionConstructor.Invoke(new object[] { });

                string[] segmentUrl = request.Url.AbsolutePath
                    .Split('/', StringSplitOptions.RemoveEmptyEntries);

                object[] parametters = new object[routeInfo.ParamNames.Count()];
                int index = 0;
                foreach (var param in routeInfo.ParamSegments)
                {
                    if (param >= 0)
                    {
                        parametters[param] = segmentUrl[index];
                    }
                    index++;
                }

                string[] segmentQuery = request.Url.Query.TrimStart('?')
                    .Split('&', StringSplitOptions.RemoveEmptyEntries);
                if (segmentQuery.Count() > 0)
                {
                    for (int i = 0; i < parametters.Count(); i++)
                    {
                        if (parametters[i] == null &&
                            IsExistParametter(segmentQuery, routeInfo.ParamNames[i], out object queryValue))
                        {
                            parametters[i] = queryValue;
                        }
                    }
                }

                object actionValue = routeInfo.Method.Invoke(actionClassObject, parametters);
                OutputResponse(actionValue, request, response);

            }
            catch (Exception ex)
            {
                OutputResponse(ex, request, response);
            }

            return response;
        }

        private static HttpListenerResponse ExecuteMethodPut(RouteInfo routeInfo, HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                //style: with out invoke contructor 
                //object actionType = Activator.CreateInstance(routeInfo.Action);
                Type actionType = routeInfo.Action;
                ConstructorInfo actionConstructor = actionType.GetConstructor(Type.EmptyTypes);
                object actionClassObject = actionConstructor.Invoke(new object[] { });

                string[] segmentUrl = request.Url.AbsolutePath
                    .Split('/', StringSplitOptions.RemoveEmptyEntries);

                object[] parametters = new object[routeInfo.ParamNames.Count()];
                int index = 0;
                foreach (var param in routeInfo.ParamSegments)
                {
                    if (param >= 0)
                    {
                        parametters[param] = segmentUrl[index];
                    }
                    index++;
                }

                string[] segmentQuery = request.Url.Query.TrimStart('?')
                    .Split('&', StringSplitOptions.RemoveEmptyEntries);
                if (segmentQuery.Count() > 0)
                {
                    for (int i = 0; i < parametters.Count(); i++)
                    {
                        if (parametters[i] == null &&
                            IsExistParametter(segmentQuery, routeInfo.ParamNames[i], out object queryValue))
                        {
                            parametters[i] = queryValue;
                        }
                    }
                }

                object actionValue = routeInfo.Method.Invoke(actionClassObject, parametters);
                OutputResponse(actionValue, request, response);

            }
            catch (Exception ex)
            {
                OutputResponse(ex, request, response);
            }

            return response;
        }

        private static HttpListenerResponse ExecuteMethodDelete(RouteInfo routeInfo, HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                //style: with out invoke contructor 
                //object actionType = Activator.CreateInstance(routeInfo.Action);
                Type actionType = routeInfo.Action;
                ConstructorInfo actionConstructor = actionType.GetConstructor(Type.EmptyTypes);
                object actionClassObject = actionConstructor.Invoke(new object[] { });

                string[] segmentUrl = request.Url.AbsolutePath
                    .Split('/', StringSplitOptions.RemoveEmptyEntries);

                object[] parametters = new object[routeInfo.ParamNames.Count()];
                int index = 0;
                foreach (var param in routeInfo.ParamSegments)
                {
                    if (param >= 0)
                    {
                        parametters[param] = segmentUrl[index];
                    }
                    index++;
                }

                string[] segmentQuery = request.Url.Query.TrimStart('?')
                    .Split('&', StringSplitOptions.RemoveEmptyEntries);
                if (segmentQuery.Count() > 0)
                {
                    for (int i = 0; i < parametters.Count(); i++)
                    {
                        if (parametters[i] == null &&
                            IsExistParametter(segmentQuery, routeInfo.ParamNames[i], out object queryValue))
                        {
                            parametters[i] = queryValue;
                        }
                    }
                }

                object actionValue = routeInfo.Method.Invoke(actionClassObject, parametters);
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
