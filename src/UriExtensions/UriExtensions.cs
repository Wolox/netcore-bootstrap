
using System;
using System.Web;

namespace EduitorNetCore.UriExtensionMethods
{
    public static class UriExtensions
    {
        public static Uri AddParameter(this Uri url, string paramName, string paramValue)
        {
            var uriBuilder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query[paramName] = paramValue;
            uriBuilder.Query = query.ToString();
            return uriBuilder.Uri;
        }
    }
}
