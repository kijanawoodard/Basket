using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Basket.Web.Models;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Indexes;
using Raven.Client.NodaTime;
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
            LoadData(store);
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

        private void LoadData(IDocumentStore store)
        {
            var filename = @"product_data.txt";
            var directory = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            var path = Path.Combine(directory, filename);
            var raw = File.ReadAllText(path);
            var lines = raw.Split(new[] {"EOL:EOL\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            var products = ParseProducts(lines);

            using (var session = store.OpenSession())
            {
                var catalog = new ProductCatalog {Id = "catalogs/fall"};
                catalog.Products = products.ToArray();

                session.Store(catalog);
                session.SaveChanges();
            }
        }

        private IEnumerable<Product> ParseProducts(string[] lines)
        {
            foreach (var line in lines)
            {
                var columns = line.Split('\t');
                if (columns[0] != "Customer") continue;
                yield return new Product
                {
                    ProductId = columns[6],
                    Description = columns[9],
                    Categories = new [] { columns[8] },
                    ImageUrl = $"http://us.longaberger.com/images/items/{columns[10]}",
                    LongDescription = columns[11],
                    Skus = new []
                    {
                        new Sku
                        {
                            Available = true,
                            Code = columns[6],
                            Description = columns[9],
                            ImageUrl = $"http://us.longaberger.com/images/items/{columns[10]}",
                            Price = decimal.Parse(columns[1])
                        }
                    }
                };
            }
        }

        private IEnumerable<Product> _products = new[]
        {
            new Product
            {
                ProductId = "183313",
                Categories = new [] {"Baskets & Accessories"},
                Description = "Team U.S.A. Basket & Protector Set",
                LongDescription = "CELEBRATE AMERICA! Show your American pride with our Team USA Basket and Protector Set. Available for a limited time to coincide with the summer games, this beautiful basket features Bold Red, White, Navy Blue and Warm Brown – the Team U.S.A Basket is a true winner! Made in the U.S.A. Includes Basket (10 3/4\"l x 7 3/4\"w x 5 1/4\"h; Rec. Wt. Use: 10 lbs.) and Protector.",
                ImageUrl = "http://us.longaberger.com/images/items/63606.jpg",
                Skus = new []
                {
                    new Sku
                    {
                        Code = "183313",
                        Description = "Team U.S.A. Basket & Protector Set",
                        Price = 75.00m,
                        ImageUrl = "http://us.longaberger.com/images/items/63606.jpg",
                        Available = true
                    } 
                }
            },
            new Product
            {
                ProductId = "2346",
                Categories = new [] {"Baskets & Accessories", "Spring & Summer 2016 WishList"},
                Description = "Cake Basket with Small Riser",
                LongDescription = @"J.W. Longaberger crafted the Cake Basket for his wife, Bonnie, and for the ladies of Dresden, Ohio, to carry their homemade pies to church socials and picnics. Two Woven Traditions® Grandma Bonnie's™ Pie Plates fit easily inside with the Small Riser. Be sure to add a Protector keep your basket safe from spills. Carries our Woven Traditions 8"" x 8"" Baking Dish, Grandma Bonnie's™ Pie Plate, 8-in-1 Large Bowl and many more. Protector and WoodCrafts Lid sold separately. Made in the U.S.A. 12""l x 12""w x 6""h Rec. Wt. Use: 30 lbs.

The Cake Protector is available to order with an extended delivery of July.",
                ImageUrl = "http://us.longaberger.com/images/items/17300BSKT.jpg",
                Skus = new []
                {
                    new Sku
                    {
                        Code = "1730090",
                        Description = "Pewter",
                        Price = 87.00m,
                        ImageUrl = "http://us.longaberger.com/images/items/1730090.jpg",
                        Available = true
                    },
                    new Sku
                    {
                        Code = "1730015",
                        Description = "Vintage",
                        Price = 87.00m,
                        ImageUrl = "http://us.longaberger.com/images/items/1730015.jpg",
                        Available = true
                    },
                    new Sku
                    {
                        Code = "1730039",
                        Description = "Warm Brown",
                        Price = 87.00m,
                        ImageUrl = "http://us.longaberger.com/images/items/1730039.jpg",
                        Available = true
                    }
                }
            }

        };
    }
}
