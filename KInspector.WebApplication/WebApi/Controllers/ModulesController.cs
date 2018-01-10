using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Kentico.KInspector.Core;
using Microsoft.Web.Administration;
using System.Collections.Generic;

namespace Kentico.KInspector.WebApplication.WebApi.Controllers
{
    public class ModulesController : ApiController
	{
		/// <summary>
		/// Categories, which have their own page in the UI. The rest
		/// is displayed together on the 'Analysis' page.
		/// </summary>
		public static string[] SeparateCategories = { "Security", "Setup" };


		/// <summary>
		/// GET api/modules/GetModulesMetadata
		/// </summary>
		[ActionName("GetModulesMetadata")]
		public HttpResponseMessage GetModulesMetadata([FromUri]InstanceConfig config, [FromUri] string category = null)
		{
			try
			{
				var instance = new InstanceInfo(config);
				var version = instance.Version;

				// Get all modules of given version
				var modules = ModuleLoader.Modules
					.Select(x => x.GetModuleMetadata())
					.Where(x => x.SupportedVersions.Contains(version));

				// Filter modules by category - return either specified category, or the rest
				if (string.IsNullOrEmpty(category))
				{
					foreach (var separateCategory in SeparateCategories)
					{
						modules = modules.Where(x => x.Category == null || !x.Category.StartsWith(separateCategory, StringComparison.InvariantCultureIgnoreCase));
					}
				}
				else
				{
					modules = modules.Where(x => x.Category != null && x.Category.StartsWith(category, StringComparison.InvariantCultureIgnoreCase));
				}

				if (!modules.Any())
				{
					return Request.CreateResponse(HttpStatusCode.BadRequest, $"There are no modules available for version {version}.");
				}

				return Request.CreateResponse(HttpStatusCode.OK, modules);
			}
			catch (Exception e)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
			}
		}


		/// <summary>
		/// GET api/modules/GetSetupModulesMetadata
		/// </summary>
		[ActionName("GetSetupModulesMetadata")]
		public HttpResponseMessage GetSetupModulesMetadata([FromUri]InstanceConfig config)
		{
			try
			{
				var instance = new InstanceInfo(config);
				var version = instance.Version;

				// Get all modules of given version which are in 'Setup' category
				var modules = ModuleLoader.Modules
					.Select(x => x.GetModuleMetadata())
					.Where(x => x.SupportedVersions.Contains(version))
					.Where(x => x.Category != null && x.Category.StartsWith("Setup", StringComparison.InvariantCultureIgnoreCase));

				if (!modules.Any())
				{
					return Request.CreateResponse(HttpStatusCode.BadRequest, $"There are no modules available for version {version}.");
				}

				return Request.CreateResponse(HttpStatusCode.OK, modules);
			}
			catch (Exception e)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
			}
		}


		/// <summary>
		/// GET api/modules/GetModuleResult
		/// </summary>
		[ActionName("GetModuleResult")]
		public HttpResponseMessage GetModuleResult(string moduleName, [FromUri]InstanceConfig config)
		{
			try
			{
				var instance = new InstanceInfo(config);
				var result = ModuleLoader.GetModule(moduleName).GetResults(instance);

				return Request.CreateResponse(HttpStatusCode.OK, result);
			}
			catch (Exception e)
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, $"Error in \"{moduleName}\" module. Error message: {e.Message}");
			}
		}


		/// <summary>
		/// Get api/modules/GetKenticoVersion
		/// </summary>
		[ActionName("GetKenticoVersion")]
		public HttpResponseMessage GetKenticoVersion([FromUri]InstanceConfig config)
		{
			try
			{
				var instance = new InstanceInfo(config);
				var version = instance.Version;
				return Request.CreateResponse(HttpStatusCode.OK, version);
			}
			catch (Exception e)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
			}
		}

		/// <summary>
		/// Get api/modules/GetIISSites
		/// </summary>
		[ActionName("GetIISSites")]
		public HttpResponseMessage GetIISSites([FromUri]InstanceConfig config)
		{
			try
			{
				var instance = new InstanceInfo(config);
				ServerManager server = new ServerManager();
				var sites = server.Sites.Where(site => site.State == ObjectState.Started);
				var siteNames = new List<Tuple<string, string>>();
				foreach(var site in sites)
				{
					//Get the Binding objects for this Site
					BindingCollection bindings = site.Bindings;
					var firstBinding = bindings.FirstOrDefault();
					if (firstBinding != null)
					{
						siteNames.Add(new Tuple<string, string>(site.Name, firstBinding.Host));
					}
				}
				return Request.CreateResponse(HttpStatusCode.OK, siteNames.OrderBy(item => item.Item1));
			}
			catch (Exception e)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
			}
		}
	}
}