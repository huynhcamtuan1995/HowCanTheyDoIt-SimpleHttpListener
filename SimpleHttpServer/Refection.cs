using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleHttpServer
{
    public static class Refection
    {
        private static Dictionary<string, Type> FunctionDictionary = new Dictionary<string, Type>();
        public static void RegisterFunction()
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
                BaseFunction function = (BaseFunction)Activator.CreateInstance(FunctionDictionary[request.Function]);
                function.Initialize(request);
                function.Execute();
                function.GetResponse(out response);
            }
        }

    }
}
