using Microsoft.Web.Administration;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace Kentico.KInspector.Core
{
    public class InstanceInfo : IInstanceInfo
    {
        private Lazy<DatabaseService> dbService;
        private Lazy<Version> version;
        private Lazy<Uri> uri;
        private Lazy<DirectoryInfo> directory;


        /// <summary>
        /// URI of the application instance
        /// </summary>
        public Uri Uri => uri.Value;

        /// <summary>
        /// Directory of the application instance
        /// </summary>
        public DirectoryInfo Directory => directory.Value;

        /// <summary>
        /// Version of the instance based on the database setting key.
        /// </summary>
        public Version Version => version.Value;

        /// <summary>
        /// Configuration with instance information.
        /// </summary>
        public InstanceConfig Config { get; private set; }


        /// <summary>
        /// Database service to communicate with the instance database.
        /// </summary>
        public IDatabaseService DBService => dbService.Value;

        /// <summary>
        /// Creates instance information based on configuration.
        /// </summary>
        /// <param name="config">Instance configuration</param>
        public InstanceInfo(InstanceConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("version");
            }

			if(config.IISSiteName != null)
			{
				ServerManager server = new ServerManager();
				SiteCollection sites = server.Sites;
				var site = sites.FirstOrDefault(item => item.Name == config.IISSiteName);

				if(site != null && site.State == ObjectState.Started)
				{
					//Get the Binding objects for this Site
					BindingCollection bindings = site.Bindings;
					var firstBinding = bindings.FirstOrDefault();
					if(firstBinding != null)
					{
						config.Url = firstBinding.Host;
					}

					//Get the list of all Applications for this Site
					var firstApp = site.Applications.FirstOrDefault();
					if(firstApp != null)
					{
						var firstVirApp = firstApp.VirtualDirectories.FirstOrDefault();
						config.Path = firstVirApp.PhysicalPath;
					}

					Configuration webConfig = site.GetWebConfiguration();
					ConfigurationSection connectionStringsSection = webConfig.GetSection("connectionStrings");
					var collection = connectionStringsSection.GetCollection();
					var connectionString = collection.FirstOrDefault(item => item.Attributes["name"] != null && item.Attributes["name"].Value.ToString() == "CMSConnectionString");
					var sqlConn = new SqlConnectionStringBuilder(connectionString.Attributes["connectionString"].Value.ToString());
					config.IntegratedSecurity = sqlConn.IntegratedSecurity;
					config.Server = sqlConn.DataSource;
					config.Database = sqlConn.InitialCatalog;
					config.User = sqlConn.UserID;
					config.Password = sqlConn.Password;
				}
			}


            Config = config;

            dbService = new Lazy<DatabaseService>(() => new DatabaseService(Config));
            version = new Lazy<Version>(GetKenticoVersion);

            // Ensure backslash to the Config.Url to support VirtualPath URLs.
            // Sometimes the website is running under virtual path and 
            // the URL looks like this http://localhost/kentico8
            // Some modules (RobotsTxtModule, CacheItemsModle, ...) try 
            // to append the relative path to the base URL but
            // without trailing slash, the relative path is replaced.
            // E.g.: 
            //      var uri = new Uri("http://localhost/kentico8");
            //      new Uri(uri, "robots.txt"); -> http://localhost/robots.txt
            // 
            // With trailing slash, the relative path is appended as expected.
            //      var uri = new Uri("http://localhost/kentico8/");
            //      new Uri(uri, "robots.txt"); -> http://localhost/kentico8/robots.txt
            uri = new Lazy<Uri>(() => new Uri(Config.Url.EndsWith("/") ? Config.Url : Config.Url + "/"));
            directory = new Lazy<DirectoryInfo>(() => new DirectoryInfo(Config.Path));
        }


        /// <summary>
        /// Gets the version of Kentico.
        /// </summary>
        private Version GetKenticoVersion()
        {
            string version = DBService.ExecuteAndGetScalar<string>("SELECT KeyValue FROM CMS_SettingsKey WHERE KeyName = 'CMSDBVersion'");
            return new Version(version);
        }
    }
}
