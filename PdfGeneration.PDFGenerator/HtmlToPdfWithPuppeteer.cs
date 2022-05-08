using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace PdfGeneration.HtmlToPdf
{
    public class HtmlToPdfWithPuppeteer
    {
        public static async Task<Stream> Convert(string htmlAsAstring, ILogger log)
        {
            log.LogInformation("Converting to pdf using PuppeteerSharp");
            
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            await using var page = await browser.NewPageAsync();
            await page.SetContentAsync(htmlAsAstring);

            var stream = await page.PdfStreamAsync();

            stream.Position = 0;

            return stream;
        }
    }
}
