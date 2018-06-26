using System;
using System.Web;
using System.Web.Mvc;

using System.Web.Security;

namespace StaffPortal.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        //
        // GET: /Login/

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            ViewBag.message = "";
            return View("Login");
        }

        [HttpPost]
        public ActionResult Login(string username, string password, string returnUrl)
        {
            if (returnUrl != "/")
            {
                returnUrl = returnUrl.Remove(0, 23);
                returnUrl = HttpUtility.UrlDecode(returnUrl);
            }
            else
            {
                returnUrl = "/Home/Index";
            }

            if (Membership.ValidateUser(username, password))
            {
                FormsAuthentication.SetAuthCookie(username, false);

                if (!String.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else return RedirectToAction("Index", "Login");
            }
            else
            {
                ViewBag.ValidateUserMessage = "სახელი ან პაროლი არასწორია!";
                return View("Index");
            }
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }        
    }
}
