namespace KsxWebsiteToJekyll
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class KsxDocument : IKsxDocument
    {
        private readonly string[] problemStrings = { "<span", "style=", "<div>" };

        public List<string> Tags { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public string Content { get; set; }

        public IEnumerable<ConversionProblem> Problems
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Content))
                {
                    return new List<ConversionProblem>();
                }

                return
                    this.Content.Split(Environment.NewLine.ToCharArray())
                        .Select((elem, i) => new ConversionProblem(i, elem))
                        .Where(line => problemStrings.Any(s => line.Line.Contains(s)));
            }
        }
    }
}