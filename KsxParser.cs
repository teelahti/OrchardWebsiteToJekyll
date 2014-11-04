namespace KsxWebsiteToJekyll
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;

    using Fizzler.Systems.HtmlAgilityPack;

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
            // TODO: If content item is not known, write an error
            // TODO: Parse comments
            // TODO: Remove span .mso -extras

            return result;
        }

        static string StripHeaderAndFooterFromContent(this string content)
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