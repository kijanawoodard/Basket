using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Basket.Web.Models;
using Raven.Client;

namespace Basket.Web.Controllers
{
    public class ProductSearchController : Controller
    {
        private readonly IDocumentSession _session;

        public ProductSearchController(IDocumentSession session)
        {
            _session = session;
        }

        [HttpGet]
        public ActionResult Index(string q)
        {
            var products = _session.Query<ProductSearchIndex.Query, ProductSearchIndex>()
                .Search(x => x.Search, q)
                .ProjectFromIndexFieldsInto<ProductSearchIndex.Result>()
                .ToList();

            var model = new ViewModel
            {
                Query = q,
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
            public string Query { get; set; }
            public List<ProductSearchIndex.Result> Products { get; set; }
            public List<string> Categories { get; set; }
        }
    }
}