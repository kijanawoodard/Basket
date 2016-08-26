using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Basket.Web.Models;
using Raven.Client;

namespace Basket.Web.Controllers
{
    public class ProductsByCategoryController : Controller
    {
        private readonly IDocumentSession _session;

        public ProductsByCategoryController(IDocumentSession session)
        {
            _session = session;
        }

        [HttpGet]
        public ActionResult Index(string category = "Specials")
        {
            var catalog = _session.Load<ProductCatalog>("catalogs/fall");
            var products = catalog.Products.Where(x => x.Categories.Contains(category)).ToList();
            var model = new ViewModel
            {
                Category = category,
                Products = products,
                Categories = new List<string>
                {
                    "Specials",
                    "August Flyer",
                    "WOVEN™",
                    "Spring & Summer 2016 WishList",
                    "Baskets & Accessories",
                    "Everyday Tableware™",
                    "Woven Traditions® Pottery",
                    "Home Accents",
                    "Tabletop",
                    "Collectors Club Limited",
                    "Collectors Club",
                    "Past Features",
                    /*"Homestead Online",*/
                    "State Baskets for John Rochon, Jr. Events",
                    "Longaberger Couture™",
                    "Host & Booking Accessories"
                }
            };
            return View(model);
        }

        public class ViewModel
        {
            public string Category { get; set; }
            public List<Product> Products { get; set; }
            public List<string> Categories { get; set; }
        }
    }
}