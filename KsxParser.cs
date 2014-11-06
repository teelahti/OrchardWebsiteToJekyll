namespace KsxWebsiteToJekyll
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;

    using Fizzler.Systems.HtmlAgilityPack;

    using HtmlAgilityPack;

    internal static class KsxParser
    {
        internal static IKsxDocument Parse(SourceDocument source)
        {
            var document = source.Document.DocumentNode;
            var date = document.QuerySelector("div.published");
            IKsxDocument result;

            // Ugly strategy pattern...
            if (date == null)
            {
                // --> page
                result = new KsxPage();
            }
            else
            {
                // --> blog post
                var blogPost = new KsxBlogPost { 
                                                   Date = DateTime.Parse(date.InnerText, new CultureInfo("fi-FI"))
                                               };

                // Parse blog comments
                var commentsNode = document.QuerySelector("#comments");
                if (commentsNode != null)
                {
                    var commentsDoc = new HtmlDocument();
                    commentsDoc.LoadHtml(commentsNode.OuterHtml);

                    var node = commentsDoc.DocumentNode;

                    var whos = node.QuerySelectorAll(".who").Select(ExtractText).ToList();
                    var whens = node.QuerySelectorAll("time").Select(s => s.Attributes["datetime"].Value).ToList();
                    var texts = node.QuerySelectorAll(".text").Select(ExtractText).ToList();

                    // TODO: Include all required disqus comments meta information in post https://help.disqus.com/customer/portal/articles/472150-custom-xml-import-format
                    var comments = whos.Zip(whens, (who, when) => new BlogComment(who, when, null))
                            .Zip(texts, (comment, s) =>
                            {
                                comment.Text = s;
                                return comment;
                            });

                    blogPost.Comments.AddRange(comments);
                }

                result = blogPost;
            }

            result.Title = HttpUtility.HtmlDecode(document.QuerySelector("#layout-main h1:first-child").InnerText);
            result.Slug = source.FileInfo.Name;

            // This is CPU intensive operation
            result.Content =
                document.QuerySelector("#layout-main").InnerHtml
                    .StripHeaderAndFooterFromContent()
                    .ConvertToMarkdown();

            // TODO: Replace image paths

            // Parse tags
            result.Tags = document.QuerySelectorAll("p.tags>a")
                .Select(t => HttpUtility.HtmlDecode(t.InnerHtml))
                .ToList();

            // TODO: Parse galleries, use http://dimsemenov.com/plugins/magnific-popup/documentation.html

            return result;
        }

        private static string ExtractText(HtmlNode s)
        {
            return HttpUtility.HtmlDecode(s.InnerText).Trim();
        }

        private static string StripHeaderAndFooterFromContent(this string content)
        {
            const string HeaderEndTag = "</header>";
            var headerEnd = content.IndexOf(HeaderEndTag, StringComparison.OrdinalIgnoreCase) + HeaderEndTag.Length;

            const string FooterStartTag = "<p class=\"tags\">";
            var footerStart = content.IndexOf(FooterStartTag, StringComparison.OrdinalIgnoreCase);

            if (footerStart == -1)
            {
                footerStart = content.IndexOf("<div id=\"comments\">", StringComparison.OrdinalIgnoreCase);
            }

            if (footerStart == -1)
            {
                footerStart = content.IndexOf("<form", StringComparison.OrdinalIgnoreCase);
            }

            return content.Substring(headerEnd, footerStart - headerEnd);
        }

        static string ConvertToMarkdown(this string htmlContent)
        {
            // Pandoc installs by default on local appdata. Need to find the full path 
            // for the process to work with UseShellExecute = false setting.
            string localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");

            if (string.IsNullOrWhiteSpace(localAppData))
            {
                throw new Exception("Cannot find Pandoc installation as LOCALAPPDATA env variable is missing.");
            }

            string processName = Path.Combine(localAppData, @"pandoc\pandoc.exe");
            string args = String.Format(@"-r html -t markdown");

            var psi = new ProcessStartInfo(processName, args)
                      {
                          RedirectStandardOutput = true,
                          RedirectStandardInput = true,
                          UseShellExecute = false
                      };

            var p = new Process { StartInfo = psi };

            p.Start();

            string outputString = "";
            byte[] inputBuffer = Encoding.UTF8.GetBytes(htmlContent);
            p.StandardInput.BaseStream.Write(inputBuffer, 0, inputBuffer.Length);
            p.StandardInput.Close();

            p.WaitForExit(2000);
            using (var sr = new StreamReader(p.StandardOutput.BaseStream))
            {
                outputString = sr.ReadToEnd();
            }

            return outputString;
        }
    }
}