using System;
using System.Web;
using System.Web.Mvc;
using Raven.Client;

namespace Basket.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDocumentSession _session;

        public HomeController(IDocumentSession session)
        {
            _session = session;
        }

        public ActionResult Index()
        {
            return RedirectToAction("Index", "ProductsByCategory", new { category = "Specials" });
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}