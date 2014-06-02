using System;
using System.Collections.Generic;
using System.Web.Http.Controllers;

namespace WebAPI.OutputCache
{
	public interface IRelatedMethodResolver
	{
		IEnumerable<string> FindAllGetMethods(Type controllerType, IEnumerable<HttpParameterDescriptor> httpParameterDescriptors);
	}
}