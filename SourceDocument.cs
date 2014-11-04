namespace KsxWebsiteToJekyll
{
    using System;
    using System.IO;
    using System.Text;

    using HtmlAgilityPack;

    internal class SourceDocument
    {
        public HtmlDocument Document { get; private set; }

        public FileInfo FileInfo { get; private set; }

        public SourceDocument(string path)
        {
            this.FileInfo = new FileInfo(path);

            var fileContents = File.ReadAllText(path, Encoding.UTF8);

            if (fileContents.StartsWith(@"<!DOCTYPE html>", StringComparison.OrdinalIgnoreCase))
            {
                this.Document = new HtmlDocument();
                this.Document.LoadHtml(fileContents);
            }
        }
    }
}