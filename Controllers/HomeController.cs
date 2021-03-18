using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SitecoreSEOAnalyzer.Models;

namespace SitecoreSEOAnalyzer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index(Home model, string sortOrder)
        {
            if (!string.IsNullOrEmpty(model.Text))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        HttpClient client = new HttpClient();
                        var response = await client.GetAsync(model.Text);
                        var pageContents = await response.Content.ReadAsStringAsync();
                        HtmlDocument pageDocument = new HtmlDocument();
                        pageDocument.LoadHtml(pageContents);

                        if (model.StopwordCheck)
                        {
                            // page occurrence
                            var getPage = pageDocument.DocumentNode.InnerHtml;
                            var pageOccurrence = CalculateStopwordOccurrence(getPage);
                            model.PageOccurrences = pageOccurrence;

                            // meta occurrence
                            var meta = new StringBuilder();
                            var getMeta = pageDocument.DocumentNode.SelectNodes("//meta");
                            foreach (var node in getMeta)
                            {
                                meta.Append(node.GetAttributeValue("content", string.Empty));
                            }
                            var metaOccurrence = CalculateStopwordOccurrence(meta.ToString());
                            model.MetaOccurrences = metaOccurrence;
                        }

                        if (model.LinkCheck)
                        {
                            // external links occurrence
                            var externalLinks = new StringBuilder();
                            var getExternalLinks = pageDocument.DocumentNode.SelectNodes("//a[@href]");
                            foreach (var node in getExternalLinks)
                            {
                                externalLinks.Append(node.GetAttributeValue("href", string.Empty)).Append("|");
                            }
                            var linkOccurrence = CalculateLinkOccurrence(externalLinks.ToString());
                            model.ExternalLinks = linkOccurrence;
                        }
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("Text", "Please try again with a different URL");
                    }
                }
            }

            return View(model);
        }

        private Dictionary<string, int> CalculateStopwordOccurrence(string text)
        {
            var stopwords = new Stopword().Stopwords;

            Dictionary<string, int> occurrence = new Dictionary<string, int>();
            char[] delimiters = { ' ', '.', ',', ';', ':', '?', '\n', '\r' };
            string[] words = text.Split(delimiters);

            foreach (string word in words)
            {
                string w = word.Trim().ToLower();
                if (stopwords.ContainsKey(w))
                {
                    if (!occurrence.ContainsKey(w))
                    {
                        occurrence.Add(w, 1);
                    }
                    else
                    {
                        occurrence[w] += 1;
                    }
                }
            }

            return occurrence;
        }

        private Dictionary<string, int> CalculateLinkOccurrence(string text)
        {
            Dictionary<string, int> occurrence = new Dictionary<string, int>();
            char[] delimiters = { '|' };
            string[] words = text.Split(delimiters);

            foreach (var word in words)
            {
                string w = word.Trim().ToLower();
                if (w.Contains("http"))
                {
                    if (!occurrence.ContainsKey(w))
                    {
                        occurrence.Add(w, 1);
                    }
                    else
                    {
                        occurrence[w] += 1;
                    }
                }
            }

            return occurrence;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
