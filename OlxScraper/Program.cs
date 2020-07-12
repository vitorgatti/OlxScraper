using OlxScraper.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OlxScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Informe o que está procurando:");
            var search = Console.ReadLine();

            Console.WriteLine("\nInforme o estado (somente a sigla):");
            var state = Console.ReadLine().ToLower();

            var urlState = string.Format("https://{0}.olx.com.br/", state);
            var regionDictionary = GetRegionsFromStateUrl(urlState);

            Console.WriteLine("\nInforme o código da região:");
            var ddd = Convert.ToInt32(Console.ReadLine());

            var urlRegion = regionDictionary.FirstOrDefault(a => a.Key == ddd).Value;
            RecursiveScrapySalesShowcase(search, urlRegion);
        }

        private static Dictionary<int, string> GetRegionsFromStateUrl(string urlState)
        {
            var regionDictionary = new Dictionary<int, string>();
            var dddRegexMatch = @"(D)+ [\d]+ - ";
            var regionAttribute = "data-lurker_region";
            var zoneAttribute = "data-lurker_zone";
            var pathListItensLink = "//ul//li//a";
            int code = 0;

            var htmlDocument = Web.ReturnHtmlDocumentFromUrl(urlState);
            var nodesRegions = htmlDocument.DocumentNode
                .SelectNodes(pathListItensLink)
                .Where(a => a.Attributes.Any(
                        a => a.Name == regionAttribute ||
                        a.Name == zoneAttribute));

            Console.WriteLine("\nRegiões disponíveis para o estado informado:");

            foreach (var region in nodesRegions)
            {
                code++;
                Console.WriteLine(string.Format("{0} -{1}", code, Regex.Replace(region.InnerText, dddRegexMatch, "")));

                regionDictionary.Add(
                    code,
                    region.GetAttributeValue("href", string.Empty)
                );
            }

            return regionDictionary;
        }

        private static void RecursiveScrapySalesShowcase(string search, string urlRegion, int page = 1)
        {
            var listSales = "//ul[@id='ad-list']//li/a";
            var nodePrice = ".//div[2]/p";
            var nodeLocal = "//p[@class='fnmrjs-13 hdwqVC']";
            var nodeNextPage = "//a[@data-lurker-detail='next_page']";

            var url = page != 1 ? string.Format("{0}?o={1}&q={2}", urlRegion, page, search) :
                      string.Format("{0}?q={1}", urlRegion, search);

            var htmlDocument = Web.ReturnHtmlDocumentFromUrl(url);
            var nodeSales = htmlDocument.DocumentNode.SelectNodes(listSales);

            var nextPage = htmlDocument.DocumentNode.SelectSingleNode(nodeNextPage)?.InnerText;

            foreach (var sale in nodeSales)
            {
                Console.WriteLine(
                    string.Format("\nTitle:{0}", sale.GetAttributeValue("title", string.Empty)) +
                    string.Format("\nPrice:{0}", sale.SelectSingleNode(nodePrice)?.InnerText) +
                    string.Format("\nLocal:{0}", sale.SelectSingleNode(nodeLocal)?.InnerText) +
                    string.Format("\nUrl:{0}", sale.GetAttributeValue("href", string.Empty)));
            }

            if (nextPage != null)
            {
                page++;
                RecursiveScrapySalesShowcase(search, urlRegion, page);
                //ATENÇÃO, dependendo do número de páginas é recomendado colocar 
                //um sleep, para não fazer vários acessos ao site de uma vez.
                //Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }
    }

}
