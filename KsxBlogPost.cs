namespace KsxWebsiteToJekyll
{
    using System;
    using System.Globalization;

    internal class KsxBlogPost : KsxDocument
    {
        public DateTime Date { get; set; }

        public string NewSlug
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture, 
                    "{0:yyyy-MM-dd}-{1}", 
                    this.Date, 
                    this.Slug);
            }
        }

        public override string ToString()
        {
            // ---
            // layout: article
            // title: "Sample Post Style Guide"
            // categories: articles
            // modified: 2014-08-27T11:57:41-04:00
            // tags: [sample]
            // toc: true
            // comments: true
            // ads: true
            // ---
            // Content here
            return 
                string.Format(CultureInfo.InvariantCulture, 
                    "---\r\n" +
                    "layout: article \r\n" +
                    "title: \"{0}\" \r\n" + 
                    "categories: \r\n" +
                    "tags: [{1}]\r\n"+
                    "toc: false \r\n" +
                    "comments: true \r\n" +
                    "ads: false \r\n" +
                    "---\r\n\r\n" +
                    "{2}",
                    this.Title,
                    string.Join(",", this.Tags),
                    this.Content);
        }

    }
}