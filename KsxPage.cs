namespace KsxWebsiteToJekyll
{
    using System.Globalization;

    internal class KsxPage : KsxDocument
    {
        public override string ToString()
        {
            return
                string.Format(CultureInfo.InvariantCulture,
                    "---\r\n" +
                    "layout: article \r\n" +
                    "title: \"{0}\" \r\n" +
                    "permalink: {1} \r\n" +
                    "tags: [{2}]\r\n" +
                    "toc: false \r\n" +
                    "comments: false \r\n" +
                    "ads: false \r\n" +
                    "---\r\n\r\n" +
                    "{3}",
                    this.Title,
                    "/" + this.Slug + "/",
                    string.Join(",", this.Tags),
                    this.Content);
        }
    }
}