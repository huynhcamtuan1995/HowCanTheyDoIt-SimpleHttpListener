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


    public class RouteActioner
    {


        MethodActioner restMethodActioner = new MethodActioner();
        public async Task<bool> ActionRequest(HttpListenerContext context, IList<Type> handlers)
        {
            return await Task.Run(async () =>
            {
                object matchingHandler = null;

                //1. try and find the handler who has same base address as the request url
                //   if we find a handler, go to step 2, otherwise try successor
                //2. find out what verb is being used

                var httpMethod = context.Request.HttpMethod;
                var url = context.Request.RawUrl;
                bool result = false;

                var routeResult = await restMethodActioner.FindHandler(
                    typeof(IRouteHandler), context, handlers);

                if (routeResult.Handler != null)
                {
                    //handler is using RouteBase, so fair chance it is a VerbHandler
                    //var genericArgs = GetDynamicRouteHandlerGenericArgs(routeResult.Handler.GetType());

                    //MethodInfo method = typeof(ClassActions).GetMethod("DispatchToHandler",
                    //    BindingFlags.NonPublic | BindingFlags.Instance);

                    //MethodInfo generic = method.MakeGenericMethod(genericArgs[0], genericArgs[1]);
                    //result = await (Task<bool>)generic.Invoke(this, new object[]
                    //{
                    //    context, routeResult.Handler, httpMethod, url
                    //});

                    //return result;

                    return false;

                }

                //result = await this.Successor.ActionRequest(context, handlers);
                //return result;
                return false;
            });
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
                        RouteBaseAttribute[] routeBase = (RouteBaseAttribute[])handler.GetCustomAttributes(typeof(RouteBaseAttribute));

                        bool isBaseMatch = false;
                        //isBaseMatch = routeBase.Any(x =>
                        //    url.StartsWith(x.UrlBase, StringComparison.CurrentCultureIgnoreCase)) ||
                        //    url.StartsWith(Regex.Replace(handler.Name, @"(Action)\z", string.Empty),
                        //        StringComparison.CurrentCultureIgnoreCase);

                        isBaseMatch = routeBase.Any((x) =>
                        {
                            if (url.StartsWith(x.UrlBase, StringComparison.CurrentCultureIgnoreCase))
                            {

                                return true;
                            }
                            return false;
                        });





                        bool isUrlMatch = false;
                        isUrlMatch = routeBase.Any(x =>
                            IsUrlMatch(x.UrlBase, url, httpMethod)) ||
                            IsUrlMatch(Regex.Replace(handler.Name, @"(Action)\z", string.Empty), url, httpMethod);


                        if (isBaseMatch && isUrlMatch)
                        {
                            result.Handler = handler;
                            break;
                        }

                    }
                }
                return result;
            });

        }
    }
}
