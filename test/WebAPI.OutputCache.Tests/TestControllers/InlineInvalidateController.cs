using System.Web.Http;

namespace WebAPI.OutputCache.Tests.TestControllers
{
    public class InlineInvalidateController : ApiController
    {
        [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100)]
        public string Get_c100_s100()
        {
            return "test";
        }

        [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100)]
        public string Get_c100_s100_with_param(int id)
        {
            return "test";
        }

        [ActionName("getById")]
        [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100)]
        public string Get_c100_s100(int id)
        {
            return "test";
        }

        [CacheOutput(ServerTimeSpan = 50)]
        public string Get_s50_exclude_fakecallback(int id = 0, string callback = null, string de = null)
        {
            return "test";
        }

        [HttpGet]
        [CacheOutput(AnonymousOnly = true, ClientTimeSpan = 50, ServerTimeSpan = 50)]
        public string etag_match_304()
        {
            return "value";
        }

        public void Post()
        {
            Configuration.CacheOutputConfiguration().ClearCache(typeof(InlineInvalidateController), "Get_c100_s100");

            //do nothing
        }

        public void Put()
        {            
			Configuration.CacheOutputConfiguration().ClearCache((InlineInvalidateController x) => x.Get_c100_s100());
            //do nothing
        }

        public void Delete_non_standard_name()
        {
			Configuration.CacheOutputConfiguration().ClearCache((InlineInvalidateController x) => x.Get_c100_s100(7));            

			//do nothing
        }

        public void Delete_parameterized()
        {
			Configuration.CacheOutputConfiguration().ClearCache((InlineInvalidateController x) => x.Get_c100_s100_with_param(7));
			            
            //do nothing
        }
    }
}