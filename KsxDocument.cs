namespace KsxWebsiteToJekyll
{
    using System.Collections.Generic;

    internal class KsxDocument : IKsxDocument
    {
        public List<string> Tags { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public string Content { get; set; }
    }
}