using System;
using System.Net.Http;
using System.Web.Http.Filters;

namespace WebAPI.OutputCache
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class InvalidateCacheOutputAttribute : BaseCacheAttribute
    {
        private Type _controller;
        private readonly string _methodName;

        public InvalidateCacheOutputAttribute(string methodName)
            : this(methodName, null)
        {
        }

        public InvalidateCacheOutputAttribute(string methodName, Type type = null)
        {
            _controller = type;
            _methodName = methodName;
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response != null && !actionExecutedContext.Response.IsSuccessStatusCode) return;
	        _controller = _controller ?? actionExecutedContext.ActionContext.ControllerContext.Controller.GetType();

            var config = actionExecutedContext.Request.GetConfiguration();
            EnsureCache(config, actionExecutedContext.Request);

            actionExecutedContext.Request.GetConfiguration().CacheOutputConfiguration().ClearCache(_controller, _methodName);            
        }
    }
}