using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text;
using System.IO;
using System.Data.Entity.Core;
using FinaClientArea.Models;
using FinaClientArea.BusinessLogic;
using FinaClientArea;
using FinaClientArea.Enums;

namespace StaffPortal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        BusinessLogic _busineslogic;

        public BusinessLogic BisinesLogic
        {
            get
            {
                return _busineslogic ?? (_busineslogic = new BusinessLogic());
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Requests()
        {
            return View();
        }

        public ActionResult Licenses()
        {
            return View();
        }

        public ActionResult Invoices()
        {
            return View();
        }

        public ActionResult News()
        {
            return View();
        }

        public JsonResult GetCRMIncidents()
        {
            string code = "";

            if (User.Identity.IsAuthenticated)
            {
                code = User.Identity.Name;
                return Json(new { root = BisinesLogic.GetCRMIncidents(code) }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return null;
            }
        }

        public CustomJsonResult GetCurrentUser()
        {
            return this.CustomJson(() => BisinesLogic.GetCurrentUser(User.Identity.Name));
        }

        public JsonResult GetContragentLicenses()
        {
            return Json(new { root = BisinesLogic.GetContragentLicenses(User.Identity.Name) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeactivateKey(string key)
        {
            return Json(new { root = BisinesLogic.DeactivateKey(key) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateKeyComment(string key, string comment)
        {
            return Json(new { root = BisinesLogic.UpdateKeyComment(key, comment) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetContragentInvoices()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Json(new { root = BisinesLogic.GetContragentInvoices(User.Identity.Name) }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return null;
            }
        }

        public JsonResult GetNews()
        {
            return Json(new { root = BisinesLogic.GetNews() }, JsonRequestBehavior.AllowGet);
        }


        public FileResult GeneratePDF(int invoiceID)
        {
            using (var context = new FinaClientAreaEntities())
            {
                int contragentID = context.Contragents.Where(c => c.code == User.Identity.Name).ToList().Last().id;
                var generalDocEntities = context.GeneralDocs.Where(gde => gde.param_id1 == contragentID).ToList();

                generalDocEntities = generalDocEntities.Where(gde => gde.doc_type == (int)EnumDocTypes.DOC_PRODUCTOUT || gde.doc_type == (int)EnumDocTypes.DOC_SERVICEOUT).ToList();
                bool isCurrentContragentPDFOwner = generalDocEntities.Any(gde => gde.id == invoiceID);

                if (isCurrentContragentPDFOwner)
                {
                    UserModel userModel = BisinesLogic.GeneratePDF(User.Identity.Name, invoiceID);
                    string filename = "FINA_Invoice-" + User.Identity.Name + ".pdf";
                    string filepath = userModel.InvoicePath;
                    byte[] filedata = System.IO.File.ReadAllBytes(filepath);
                    string contentType = MimeMapping.GetMimeMapping(filepath);

                    System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
                    {
                        FileName = filename,
                        Inline = true,
                    };

                    Response.AppendHeader("Content-Disposition", cd.ToString());

                    return File(filedata, contentType);
                }
                else
                {
                    return null;
                }
            }                
        }
    }

    public static class ControllerExtensions
    {
        public static CustomJsonResult CustomJson<TResult>(this Controller controller, Func<TResult> callSite)
        {
            var result = new CustomJsonResult();

            try
            {
                TResult data = callSite();

                result.Data = new
                {
                    root = data,
                    success = true
                };
            }
            catch (Exception ex)
            {
                result.Data = new
                {
                    msg = ex.Message,
                    trace = ex.StackTrace,
                    success = false
                };
            }

            return result;
        }

        public static CustomJsonResult CustomPagingJson<TFilter, TResult>(this Controller controller, Func<TFilter, TResult> callSite, TFilter filter) where TFilter : BaseFilterModel
        {
            var result = new CustomJsonResult();

            try
            {
                TResult data = callSite(filter);

                result.Data = new
                {
                    root = data,
                    success = true,
                    start = filter.start,
                    limit = filter.limit,
                    count = filter.count
                };
            }
            catch (CustomValidationException ex)
            {
                result.Data = new
                {
                    msg = ex.Message,
                    trace = ex.StackTrace,
                    success = false
                };

            }
            catch (CustomException ex)
            {
                result.Data = new
                {
                    msg = ex.Message,
                    trace = ex.StackTrace,
                    success = false,
                    errorCode = ex.ErrorType,
                    noDefaultMessage = ex.NoDefaultMessage,
                    reload = ex.reload
                };

            }
            catch (OptimisticConcurrencyException ex)
            {
                result.Data = new
                {
                    msg = ex.Message,
                    trace = ex.StackTrace,
                    success = false
                };

            }
            catch (Exception ex)
            {
                result.Data = new
                {
                    msg = ex.Message,
                    trace = ex.StackTrace,
                    success = false
                };

            }

            return result;
        }
    }

    public class CustomJsonResult : JsonResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            var response = context.HttpContext.Response;

            response.ContentEncoding = System.Text.Encoding.UTF8;

            response.ContentType = "application/json";

            JsonSerializer serializer = new JsonSerializer();

            serializer.Converters.Add(new JavaScriptDateTimeConverter());

            serializer.NullValueHandling = NullValueHandling.Include;

            var sb = new StringBuilder();

            var textWriter = new StringWriter(sb);


            serializer.Serialize(new JsonTextWriter(textWriter), Data);

            response.Write(sb.ToString());
        }
    }
}