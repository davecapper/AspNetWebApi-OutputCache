using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebApi.OutputCache.Core;

namespace WebAPI.OutputCache
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class AutoInvalidateCacheOutputAttribute : BaseCacheAttribute
    {	   
	    public bool TryMatchType { get; set; }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response != null && !actionExecutedContext.Response.IsSuccessStatusCode) return;
            if (actionExecutedContext.ActionContext.Request.Method != HttpMethod.Post &&
                actionExecutedContext.ActionContext.Request.Method != HttpMethod.Put &&
                actionExecutedContext.ActionContext.Request.Method != HttpMethod.Delete &&
                actionExecutedContext.ActionContext.Request.Method.Method.ToLower() != "patch") return;

            var controller = actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerType;			            
            var config = actionExecutedContext.ActionContext.Request.GetConfiguration();
			var resolver = config.CacheOutputConfiguration().GetRelatedMethodResolver(actionExecutedContext.ActionContext.Request);			
            EnsureCache(config, actionExecutedContext.ActionContext.Request);
			var actions = resolver.FindAllGetMethods(controller, TryMatchType ? actionExecutedContext.ActionContext.ActionDescriptor.GetParameters() : null);

            foreach (var action in actions)
            {
                config.CacheOutputConfiguration().ClearCache(controller, action);                
            }
        }
    }
}