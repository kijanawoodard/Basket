using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Indexes;
using Raven.Client.NodaTime;
using Raven.Database.Config;
using Raven.Database.Server;
using SimpleInjector;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.Web.Mvc;

namespace Basket.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            RegisterContainer();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void RegisterContainer()
        {
            var container = new Container();

            RegisterRavenDB(container);
            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            container.RegisterMvcIntegratedFilterProvider();

            container.Verify();

            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
            //GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
        }

        private void RegisterRavenDB(Container container)
        {
            NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(8082);
            var store = new EmbeddableDocumentStore
            {
                UseEmbeddedHttpServer = false,
                DefaultDatabase = "baskets",
                RunInMemory = true
            };

            store.Configuration.Storage.Voron.AllowOn32Bits = true;

            store.Conventions.DefaultUseOptimisticConcurrency = true;
            store.Conventions.ShouldAggressiveCacheTrackChanges = true;
            store.Conventions.ShouldSaveChangesForceAggressiveCacheCheck = false;
            store.Conventions.MaxNumberOfRequestsPerSession = 100;
            store.Conventions.FindTypeTagName = FindTypeTagName;
            store.Initialize();
            store.ConfigureForNodaTime();

            container.Register<IDocumentStore>(() => store, Lifestyle.Singleton);
            container.Register<IDocumentSession>(() => store.OpenSession(), new WebRequestLifestyle());

            ExecuteIndexes(store);
        }

        private string FindTypeTagName(Type type)
        {
            return type.Namespace != null && type.Namespace.Contains(".Messaging.")
                ? "Events"
                : DocumentConvention.DefaultTypeTagName(type);
        }

        private void ExecuteIndexes(IDocumentStore store)
        {
            IndexCreation.CreateIndexes(Assembly.GetExecutingAssembly(), store);
        }
    }
}
