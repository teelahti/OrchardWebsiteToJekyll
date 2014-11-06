namespace KsxWebsiteToJekyll
{
    using System.Collections.Generic;

    internal interface IKsxDocument
    {
        List<string> Tags { get; set; }

        string Title { get; set; }

        string Slug { get; set; }

        string Content { get; set; }

        IEnumerable<ConversionProblem> Problems { get; }
    }
}