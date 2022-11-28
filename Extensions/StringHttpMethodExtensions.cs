using System.Net.Http;

namespace StaticWebServer.Extensions
{
    public static class StringHttpMethodExtensions
    {

        public static HttpMethod ToHttpMethod(this string methodName)
        {
            var allMethods = new HttpMethod[]
            {
                HttpMethod.Delete,
                HttpMethod.Get, 
                HttpMethod.Head, 
                HttpMethod.Options, 
                HttpMethod.Patch, 
                HttpMethod.Post, 
                HttpMethod.Put, 
                HttpMethod.Trace, 
            };

            methodName = methodName.ToLower().Trim();

            foreach (var method in allMethods)
            {
                if (method.Method.ToLower() == methodName)
                {
                    return method;
                }
            }
            return HttpMethod.Get;
        }
    }
}