using SelectPdf;


namespace PdfGeneration.HtmlToPdf
{
    public class HtmlToPdfWithSelectPdf
    {
        public static Stream Convert(string htmlAsAstring)
        {
            // instantiate a html to pdf converter object
            var converter = new SelectPdf.HtmlToPdf();

            // create a new pdf document converting an url
            PdfDocument doc = converter.ConvertHtmlString(htmlAsAstring);

            var ms = new MemoryStream();
            // save pdf document
            doc.Save(ms);

            // close pdf document
            doc.Close();

            ms.Position = 0;

            return ms;
        }
    }
}  