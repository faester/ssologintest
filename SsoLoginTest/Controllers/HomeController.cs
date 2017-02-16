using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SsoLoginTest.Controllers
{
    public class HomeController : Controller
    {
        private const string SessionCookieName = "testSessionCookie";
        
        public ActionResult Index()
        {
            ViewBag.Message = "Demonstrates some flows in SSO";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        [HttpGet]
        public ActionResult SessionCookie()
        {
            var sessionCookie = HttpContext.Request.Cookies[SessionCookieName];
            ViewBag.Message = sessionCookie == null
                ? "Currently no session cookie."
                : string.Format("Session cookie with value {0}", sessionCookie.Value);
            return View();
        }

        [HttpPost]
        public ActionResult SessionCookie(FormCollection formCollection)
        {
            var sessionCookie = HttpContext.Request.Cookies[SessionCookieName];
            HttpContext.Response.Cookies.Add(new HttpCookie(SessionCookieName, DateTime.Now.ToLongDateString()));

            return RedirectToAction("SessionCookie");
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
