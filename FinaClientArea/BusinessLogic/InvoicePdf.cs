using FinaClientArea.Models;
using Spire.Pdf;
using Spire.Pdf.HtmlConverter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Hosting;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace FinaClientArea.BusinessLogic
{
    public class InvoicePdf
    {
        public DateTime Date { get; set; }
        public string Num { get; set; }
        public Account Account { get; set; }
        public List<AgreementItem> Items { get; set; }


        public string Generate()
        {
            int index = 1;
            XElement element = new XElement("root",
                new XElement("date", Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
                new XElement("invoice_num", Num),
                new XElement("customer", Account.CompanyName),
                new XElement("customer_code", Account.CompanyCode),
                new XElement("customer_address", Account.Address),
                new XElement("summ", Items.Sum(i => i.Quantity * i.Package.Price)),
                new XElement("items", Items.Select(it => new XElement("item",
                    new XElement("loop", index++),
                    new XElement("version", it.Package.Name),
                    new XElement("quantity", it.Quantity),
                    new XElement("price", it.Package.Price),
                    new XElement("amount", it.Quantity * it.Package.Price)))));

            string file_name = GetTempFilePathWithExtension();

            try
            {
                XslCompiledTransform _transform = new XslCompiledTransform(false);
                _transform.Load(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "App_Data", "Invoice.xslt"));

                using (StringWriter sw = new StringWriter())
                {
                    _transform.Transform(new XDocument(element).CreateReader(), new XsltArgumentList(), sw);

                    using (PdfDocument pdf = new PdfDocument())
                    {
                        PdfHtmlLayoutFormat htmlLayoutFormat = new PdfHtmlLayoutFormat() { IsWaiting = false };
                        PdfPageSettings setting = new PdfPageSettings() { Size = PdfPageSize.A4 };

                        Thread thread = new Thread(() => { pdf.LoadFromHTML(sw.ToString(), false, setting, htmlLayoutFormat); });
                        thread.SetApartmentState(ApartmentState.STA);
                        thread.Start();
                        thread.Join();

                        pdf.SaveToFile(file_name);
                    }
                }
            }
            catch
            {
                file_name = string.Empty;
            }

            return file_name;
        }

        private string GetTempFilePathWithExtension()
        {
            var path = Path.GetTempPath();
            var fileName = Guid.NewGuid().ToString() + ".pdf";
            return Path.Combine(path, fileName);
        }
    }
}