using System;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;

namespace WebAPI.OutputCache
{
    public interface ICacheKeyGenerator
    {
		/// <summary>
		/// Creates a cache key for the supplied context. This method must use MakeBaseCacheKey so that all keys have the same root, which allows
		/// the cached methods to be invalidated correctly.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="mediaType"></param>
		/// <param name="excludeQueryString"></param>
		/// <returns></returns>
        string MakeCacheKey(HttpActionContext context, MediaTypeHeaderValue mediaType, bool excludeQueryString = false);
	    
		/// <summary>
	    /// Create the base cache key for the supplied context. 
	    /// </summary>
	    /// <param name="context"></param>
	    /// <returns>A string that represents the root cache key for the controller.</returns>
		string MakeBaseCacheKey(HttpActionContext context);

		string MakeBaseCacheKey(Type controllerType, string action);
    }
}
