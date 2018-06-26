using FinaClientArea.Models;
using FinaClientArea.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace FinaClientArea.BusinessLogic
{
    public static class CryptoSecurityInternal
    {
        private const string passPhrase = "B2C073734F13295722080F9ECA9F44CB";
        private const string saltValue = "721188621259BEE5C0695104F87D200D";
        private const string initVector = "@0C574D9t5W6ANN8"; // must be 16 bytes

        internal static bool IsRequestValid(SecureRequest secure, bool CheckWithDate = true)
        {
            if (secure == null)
                return false;
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(Decrypt(secure.private_key));
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));
            return ((sb.ToString() == secure.public_key) && isValitData(Decrypt(secure.private_tdate), CheckWithDate));
        }

        private static bool isValitData(string datetime, bool CheckWithDate)
        {
            if (!CheckWithDate)
                return true;
            DateTime server_date = DateTime.Now;
            DateTime recieve_date;
            if (!DateTime.TryParseExact(datetime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out recieve_date))
                return false;
            TimeSpan ts = server_date.Subtract(recieve_date);
            if (Math.Abs(ts.Hours) > 3)
                return false;
            return true;
        }

        internal static string Encrypt(string plainText)
        {
            string res = null;
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider() { KeySize = 256, Mode = CipherMode.CBC })
            {
                using (MemoryStream _stream = new MemoryStream())
                {
                    using (CryptoStream _cs = new CryptoStream(_stream, aes.CreateEncryptor(new PasswordDeriveBytes(passPhrase, Encoding.ASCII.GetBytes(saltValue), "SHA1", 10000).GetBytes(256 / 8), Encoding.ASCII.GetBytes(initVector)), CryptoStreamMode.Write))
                    {
                        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                        _cs.Write(plainTextBytes, 0, plainTextBytes.Length);
                        _cs.FlushFinalBlock();
                        _cs.Close();
                    }
                    res = Convert.ToBase64String(_stream.ToArray());
                    _stream.Close();
                }
            }
            return res;
        }

        internal static string Decrypt(string cipherText)
        {

            string res = null;
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider() { KeySize = 256, Mode = CipherMode.CBC })
            {
                using (MemoryStream _stream = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream _cs = new CryptoStream(_stream, aes.CreateDecryptor(new PasswordDeriveBytes(passPhrase, Encoding.ASCII.GetBytes(saltValue), "SHA1", 10000).GetBytes(256 / 8), Encoding.ASCII.GetBytes(initVector)), CryptoStreamMode.Read))
                    {
                        byte[] plainTextBytes = new byte[Convert.FromBase64String(cipherText).Length];
                        int decryptedByteCount = _cs.Read(plainTextBytes, 0, plainTextBytes.Length);
                        res = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                        _cs.Close();
                    }
                    _stream.Close();
                }
            }
            return res;
        }

        internal static SecureString SecureDecrypt(string cipherText)
        {

            SecureString res = new SecureString();
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider() { KeySize = 256, Mode = CipherMode.CBC })
            {
                using (MemoryStream _stream = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream _cs = new CryptoStream(_stream, aes.CreateDecryptor(new PasswordDeriveBytes(passPhrase, Encoding.ASCII.GetBytes(saltValue), "SHA1", 10000).GetBytes(256 / 8), Encoding.ASCII.GetBytes(initVector)), CryptoStreamMode.Read))
                    {
                        byte[] plainTextBytes = new byte[Convert.FromBase64String(cipherText).Length];
                        int decryptedByteCount = _cs.Read(plainTextBytes, 0, plainTextBytes.Length);
                        Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).ToList().ForEach(a => res.AppendChar(a));
                        _cs.Close();
                    }
                    _stream.Close();
                }
            }
            return res;
        }

        public static string GetMd5(string text)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(text);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));
            return sb.ToString();
        }
    }

    public class SecureRequest
    {
        public string public_key { get; set; }

        public string private_key { get; set; }

        public string private_tdate { get; set; }
    }

    public class BusinessLogic
    {
        private SecureRequest GetSecureRequest
        {
            get
            {
                string _guid = Guid.NewGuid().ToString();
                return new SecureRequest()
                {
                    public_key = CryptoSecurityInternal.GetMd5(_guid),
                    private_key = CryptoSecurityInternal.Encrypt(_guid),
                    private_tdate = CryptoSecurityInternal.Encrypt(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm:ss"))
                };
            }
        }

        FinaClientAreaEntities context = new FinaClientAreaEntities();

        public List<CRMIncidentModel> GetCRMIncidents(string code)
        {
            int contragentID = context.Contragents.Where(c => c.code == code).ToList().Last().id;

            var generalDocEntities = context.GeneralDocs.Where(gd => gd.param_id2 == contragentID).ToList();

            List<CRMIncidentModel> result = new List<CRMIncidentModel>();

            if (generalDocEntities.Count > 0)
            {
                generalDocEntities.ForEach(gd =>
                {
                    var crmIncidents = context.CRMIncidents.Where(crmi => crmi.general_id == gd.id).ToList();
                    if (crmIncidents.Count > 0)
                    {
                        crmIncidents.ForEach(ci =>
                        {
                            CRMIncidentModel crmIncidentModel = new CRMIncidentModel
                            {
                                ID = ci.id,
                                GeneralDoc_Purpose = gd.purpose,
                                GeneralDoc_Tdate = gd.tdate,
                                GeneralDoc_StatusID = gd.status_id,
                                StartDate = ci.start_date,
                                EndDate = ci.end_date,
                                CRMIncidentType_Name = context.CRMIncidentTypes.Single(cit => cit.id == ci.incident_type).name
                            };
                            
                            result.Add(crmIncidentModel);
                        });
                    }
                });
            }

            result = result.OrderByDescending(r => r.GeneralDoc_Tdate).ToList();

            return result;
        }

        public UserModel GetCurrentUser(string code)
        {
            var dbContragent = context.Contragents.Where(c => c.code == code).ToList().Last();

            UserModel result = null;

            if (dbContragent != null)
            {
                UserModel userModel = new UserModel
                {
                    ID = dbContragent.id,
                    UserName = dbContragent.name,
                    IdentCode = dbContragent.code
                };

                result = userModel;
            }

            return result;
        }

        public bool DeactivateKey(string key)
        {
            bool result = false;

            string json_request = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Serialize(new
            {
                Secure = GetSecureRequest,
                Key = CryptoSecurityInternal.Encrypt("DF6D21B4-9316-4D76-ACE6-445F301C48AC") // TODO change to {key} for final release!!!
            });

            result = Convert.ToBoolean(POSTRequest("http://license.fina.ge:64270/LicenseService/LicenseService.svc/", "DeactivateKey", json_request));

            return result;
        }

        public bool UpdateKeyComment(string key, string comment)
        {
            bool result = false;

            if(comment.Length <= 200)
            {
                string json_request = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Serialize(new
                {
                    Secure = GetSecureRequest,
                    Key = CryptoSecurityInternal.Encrypt(key),
                    Comment = comment
                });

                result = Convert.ToBoolean(POSTRequest("http://license.fina.ge:64270/LicenseService/LicenseService.svc/", "UpdateKeyComment", json_request));
            }

            return result;
        }

        public List<AccountLicenseResponse> GetContragentLicenses(string code)
        {
            List<AccountLicenseResponse> finalResult = new List<AccountLicenseResponse>();
            string json_request = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Serialize(new
            {
                Secure = GetSecureRequest,
                CompanyCode = code
            });
            string json_response = POSTRequest("http://license.fina.ge:64270/LicenseService/LicenseService.svc/", "GetAccountLicense", json_request);

            GetAccountLicenseResponse result = JsonConvert.DeserializeObject<GetAccountLicenseResponse>(json_response);
            result.license.ForEach(r => {
                r.tdate = r.deadline.ToShortDateString();

                switch (r.PackageID)
                {
                    case 1:
                        r.PackageName = "FINA ბუღალტერია";
                        break;
                    case 6:
                        r.PackageName = "FINA მაღაზია-საწყობი Professional";
                        break;
                    case 7:
                        r.PackageName = "FINA მაღაზია-საწყბი Basic";
                        break;
                    case 9:
                        r.PackageName = "FINA მაღაზია-საწყობი Remote";
                        break;
                    case 10:
                        r.PackageName = "FINA კაფე-რესტორანი";
                        break;
                    case 11:
                        r.PackageName = "FINA სასტუმრო მიმღები";
                        break;
                    case 12:
                        r.PackageName = "FINA სასტუმრო ოთახები";
                        break;
                    case 13:
                        r.PackageName = "FINA CRM";
                        break;
                    case 14:
                        r.PackageName = "FINA სიტრიბუცია Pocket PC";
                        break;
                    case 15:
                        r.PackageName = "FINA Торговля Pro";
                        break;
                    case 16:
                        r.PackageName = "FINA Бухгалтерия";
                        break;
                }

                if(r.deadline.Year.ToString() == "2201")
                {
                    r.tdate = "უვადო";
                }

                finalResult.Add(r);
            });

            finalResult = finalResult.OrderByDescending(fr => fr.id).ToList();

            return finalResult;
        }

        public List<GeneralDocModel> GetContragentInvoices(string code)
        {
            int contragentID = context.Contragents.Where(c => c.code == code).ToList().Last().id;
            var generalDocEntities = context.GeneralDocs.Where(gde => gde.param_id1 == contragentID).ToList();

            generalDocEntities = generalDocEntities.Where(gde => gde.doc_type == (int)EnumDocTypes.DOC_PRODUCTOUT || gde.doc_type == (int)EnumDocTypes.DOC_SERVICEOUT).ToList();

            List<GeneralDocModel> result = new List<GeneralDocModel>();

            generalDocEntities.ForEach(gde =>
            {
                result.Add(new GeneralDocModel
                {
                    ID = gde.id,
                    Purpose = gde.purpose,
                    TDate = gde.tdate,
                    Amount = gde.amount
                });
            });

            result = result.OrderByDescending(r => r.TDate).ToList();

            return result;
        }

        private string POSTRequest(string serviceUrl, string function_name, string json)
        {
            //this.ErrorEx = null;
            HttpWebRequest request = null;
            HttpWebResponse httpResponse = null;
            string RequestResult = string.Empty;
            Uri full_address = GenerateUrl(serviceUrl, function_name);
            if (full_address == null)
                return null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(full_address);
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.AllowAutoRedirect = false;
                request.KeepAlive = false;
                request.AllowWriteStreamBuffering = false;
                request.Timeout = 1800000;// 30 min
                request.ReadWriteTimeout = request.Timeout;
                request.ServicePoint.MaxIdleTime = request.Timeout;
                byte[] byteArray = Encoding.UTF8.GetBytes(json);
                request.ContentLength = byteArray.Length;
                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Flush();
                    dataStream.Close();
                }
                using (httpResponse = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader loResponseStream = new StreamReader(httpResponse.GetResponseStream(), Encoding.GetEncoding("utf-8")))
                    {
                        RequestResult = loResponseStream.ReadToEnd();
                        loResponseStream.Close();
                    }
                    httpResponse.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (httpResponse != null)
                {
                    httpResponse.Close();
                    httpResponse = null;
                }
                if (request != null)
                {
                    request.Abort();
                    request = null;
                }
            }
            return RequestResult;
        }


        private Uri GenerateUrl(string serviceUrl,string method_name)
        {
            string res = string.Format("{0}{1}", serviceUrl, method_name);
            return new Uri(res, UriKind.RelativeOrAbsolute);
        }

        public UserModel GeneratePDF(string code, int invoiceID)
        {
            InvoicePdf pdf = new InvoicePdf();

            var contragent = context.Contragents.Where(c => c.code == code).ToList().Last();
            var generalDoc = context.GeneralDocs.Single(gd => gd.id == invoiceID);
            pdf.Date = (DateTime)generalDoc.tdate;
            pdf.Num = generalDoc.doc_num.ToString();
            pdf.Account = new Account
            {
                CompanyName = contragent.name,
                CompanyCode = contragent.code,
                Address = contragent.address
            };
            List<AgreementItem> Items = new List<AgreementItem>();
            var productsFlows = context.ProductsFlow.Where(pf => pf.general_id == generalDoc.id).ToList();
            productsFlows.ForEach(pf =>
            {
                var product = context.Products.Single(p => p.id == pf.product_id);
                Package package = new Package();
                package.Name = product.name;
                package.Price = pf.price;
                AgreementItem item = new AgreementItem
                {
                    Package = package,
                    Quantity = pf.amount
                };
                Items.Add(item);
            });
            pdf.Items = Items;

            string invoicePath = pdf.Generate();

            return new UserModel { ID = contragent.id, IdentCode = contragent.code, UserName = contragent.name, InvoicePath = invoicePath };
        }

        public NewsResponse GetNews()
        {
            List<NewsResponse> finalResult = new List<NewsResponse>();
            string json_request = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Serialize(new
            {
                Secure = GetSecureRequest,
                //Request = new { mti = new List<int> { 2 }, c = "aaa", k = "vvv" }
                Request = new NewsFilterModel()
            });
            //string json_response = POSTRequest("http://localhost:8081/NewsService/NewsService.svc/", "GetNews", json_request);
            string json_response = POSTRequest("http://localhost:2397/NewsService.svc/", "GetNews", json_request);

            NewsResponse result = JsonConvert.DeserializeObject<NewsResponse>(json_response);

            return result;
        }
    }
}