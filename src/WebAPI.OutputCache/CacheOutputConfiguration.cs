using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using WebApi.OutputCache.Core.Cache;

namespace WebAPI.OutputCache
{
    public class CacheOutputConfiguration
    {
        private readonly HttpConfiguration _configuration;

        public CacheOutputConfiguration(HttpConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void RegisterCacheOutputProvider(Func<IApiOutputCache> provider)
        {
            _configuration.Properties.GetOrAdd(typeof(IApiOutputCache), x => provider);
        }

        public void RegisterCacheKeyGeneratorProvider<T>(Func<T> provider)
            where T: ICacheKeyGenerator
        {
            _configuration.Properties.GetOrAdd(typeof (T), x => provider);
        }		

        public void RegisterDefaultCacheKeyGeneratorProvider(Func<ICacheKeyGenerator> provider)
        {
            RegisterCacheKeyGeneratorProvider(provider);
        }

	    public void RegisterRelatedMethodResolverProvider(Func<IRelatedMethodResolver> provider)
	    {
			_configuration.Properties.GetOrAdd(typeof(IRelatedMethodResolver), x => provider);
	    }

	    public void ClearCache<T>(string action) where T : IHttpController
	    {
			ClearCache(typeof(T), action);
	    }

		public void ClearCache(Type controllerType, string action)
		{
			var generator = GetCacheKeyGenerator(null, null);
			var baseKey = generator.MakeBaseCacheKey(controllerType, action);
			var cache = GetCacheOutputProvider(null);
			if (cache.Contains( baseKey ))
			{
				cache.RemoveStartsWith(baseKey);
			}
		}

	    public void ClearCache<T,U>(Expression<Func<T, U>> expression)
	    {
			var method = expression.Body as MethodCallExpression;
			if (method == null) throw new ArgumentException("Expression is wrong");
			
			var methodName = method.Method.Name;
			var nameAttribs = method.Method.GetCustomAttributes(typeof(ActionNameAttribute), false);
			if (nameAttribs.Any())
			{
			    var actionNameAttrib = (ActionNameAttribute) nameAttribs.FirstOrDefault();
			    if (actionNameAttrib != null)
			    {
			        methodName = actionNameAttrib.Name;
			    }
			}
			
			ClearCache(typeof(T), methodName);			
	    }

        private static ICacheKeyGenerator TryActivateCacheKeyGenerator(Type generatorType)
        {
            var hasEmptyOrDefaultConstructor = 
                generatorType.GetConstructor(Type.EmptyTypes) != null || 
                generatorType.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .Any (x => x.GetParameters().All (p => p.IsOptional));
            return hasEmptyOrDefaultConstructor 
                ? Activator.CreateInstance(generatorType) as ICacheKeyGenerator 
                : null;
        }

        public ICacheKeyGenerator GetCacheKeyGenerator(HttpRequestMessage request, Type generatorType)
        {
            generatorType = generatorType ?? typeof (ICacheKeyGenerator);
            object cache;
            _configuration.Properties.TryGetValue(generatorType, out cache);

            var cacheFunc = cache as Func<ICacheKeyGenerator>;

            var generator = cacheFunc != null
                ? cacheFunc()
                : request != null 
					? request.GetDependencyScope().GetService(generatorType) as ICacheKeyGenerator :
					_configuration.DependencyResolver.GetService(generatorType) as ICacheKeyGenerator;

            return generator 
                ?? TryActivateCacheKeyGenerator(generatorType) 
                ?? new DefaultCacheKeyGenerator();
        }

        public IApiOutputCache GetCacheOutputProvider(HttpRequestMessage request)
        {
            object cache;
            _configuration.Properties.TryGetValue(typeof(IApiOutputCache), out cache);

            var cacheFunc = cache as Func<IApiOutputCache>;

            var cacheOutputProvider = cacheFunc != null ?
				cacheFunc() : 
				request != null ? 
					request.GetDependencyScope().GetService(typeof(IApiOutputCache)) as IApiOutputCache :
					_configuration.DependencyResolver.GetService(typeof(IApiOutputCache)) as IApiOutputCache;
			return cacheOutputProvider ?? new MemoryCacheDefault();
        }

		public IRelatedMethodResolver GetRelatedMethodResolver(HttpRequestMessage request)
		{
			object resolver;
			_configuration.Properties.TryGetValue( typeof( IRelatedMethodResolver ), out resolver );
			
			var resolverFunc = resolver as Func<IRelatedMethodResolver>;

			var relatedMethodResolver = resolverFunc != null
				                            ? resolverFunc()
				                            : request != null
					                              ? request.GetDependencyScope().GetService( typeof( IRelatedMethodResolver ) ) as IRelatedMethodResolver
					                              : _configuration.DependencyResolver.GetService( typeof( IRelatedMethodResolver ) ) as IRelatedMethodResolver;

			return relatedMethodResolver ?? new DefaultRelatedMethodResolver();
		}
    }
}