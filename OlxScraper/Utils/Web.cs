using HtmlAgilityPack;
using System.Net;

namespace OlxScraper.Utils
{
    public class Web
    {
        public static HtmlDocument ReturnHtmlDocumentFromUrl(string url)
        {
            using (var webClient = new WebClient())
            {
                var htmlDocument = new HtmlDocument();
                var htmlPage = webClient.DownloadString(url);
                htmlDocument.LoadHtml(htmlPage);
                return htmlDocument;
            }
        }
    }
}
