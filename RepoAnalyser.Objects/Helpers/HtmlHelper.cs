using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace RepoAnalyser.Objects.Helpers
{
    public static class HtmlHelper
    {
        public static string CleanGendarmeHtml(string html)
        {
            var unwantedTags = new[] { "script", "a" };

            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            var document = new HtmlDocument();
            document.LoadHtml(html);

            var tryGetNodes = document.DocumentNode.SelectNodes("./*|./text()");

            if (tryGetNodes == null || !tryGetNodes.Any())
            {
                return html;
            }

            var nodes = new Queue<HtmlNode>(tryGetNodes);

            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                var parentNode = node.ParentNode;

                var childNodes = node.SelectNodes("./*|./text()");

                if (childNodes != null)
                {
                    foreach (var child in childNodes)
                    {
                        nodes.Enqueue(child);
                    }
                }

                if (unwantedTags.Any(tag => tag == node.Name))
                {
                    if (childNodes != null)
                    {
                        foreach (var child in childNodes.Where(cNode => !cNode.InnerText.Contains("[show]") && !cNode.InnerText.Contains("[hide]")))
                        {
                            parentNode.InsertBefore(child, node);
                        }
                    }

                    parentNode.RemoveChild(node);

                }
            }

            document.DocumentNode.SelectSingleNode("//div[@id='Rules_block']").Remove();

            document.DocumentNode.SelectSingleNode("//div[@class='toc']").Remove();

            document.DocumentNode.SelectSingleNode("//*[normalize-space(text()) = 'List of rules used']").Remove();

            document.DocumentNode.SelectSingleNode("//style[@type='text/css']").Remove();

            return document.DocumentNode.InnerHtml.Replace("display:none", "display:block").Replace("h1", "h2");
        }
    }
}
