using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using SitecoreSEOAnalyzer.Models;

namespace SitecoreSEOAnalyzer.Controllers
{
    public class HomeController : Controller
    {
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

                        if (model.WordCheck)
                        {
                            // page occurrence
                            var getPage = pageDocument.DocumentNode.InnerHtml;
                            var pageOccurrence = CalculateOccurrence(getPage);
                            model.PageOccurrences = pageOccurrence;

                            // meta occurrence
                            var meta = new StringBuilder();
                            var getMeta = pageDocument.DocumentNode.SelectNodes("//meta");
                            foreach (var node in getMeta)
                            {
                                meta.Append(node.GetAttributeValue("content", string.Empty));
                            }
                            var metaOccurrence = CalculateOccurrence(meta.ToString());
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
                            var linkOccurrence = CalculateOccurrence(externalLinks.ToString(), "link");
                            model.ExternalLinks = linkOccurrence;
                        }
                    }
                    catch (Exception e)
                    {
                        //ModelState.AddModelError("Text", "Please try again with a different URL");
                        ModelState.AddModelError("Text", e.Message);

                    }
                }
            }

            return View(model);
        }

        private Dictionary<string, int> CalculateOccurrence(string text, string option = null)
        {
            Dictionary<string, int> occurrence = new Dictionary<string, int>();

            if (!string.IsNullOrEmpty(option) && option.Equals("link", StringComparison.OrdinalIgnoreCase))
            {
                char[] delimiters = { '|' };
                string[] words = text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                Uri uriResult;
                foreach (var word in words)
                {
                    bool isValidLink = Uri.TryCreate(word, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                    if (isValidLink)
                    {
                        if (!occurrence.ContainsKey(word))
                        {
                            occurrence.Add(word, 1);
                        }
                        else
                        {
                            occurrence[word] += 1;
                        }
                    }
                }
            }
            else
            {
                Regex wordMatcher = new Regex(@"\p{L}+");
                var words = wordMatcher.Matches(text).Select(c => c.Value);

                var stopwords = new Stopword().Stopwords;

                foreach (string word in words)
                {
                    string w = word.ToLower().Trim();

                    if (!stopwords.ContainsKey(w))
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
            }

            return occurrence;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
