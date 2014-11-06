namespace KsxWebsiteToJekyll
{
    using System;
    using System.Globalization;

    internal class BlogComment
    {
        public string Who { get; set; }

        public DateTime When { get; set; }

        public string Text { get; set; }

        public BlogComment(string who, string when, string text)
        {
            this.Who = who;
            this.When = DateTime.Parse(when, CultureInfo.GetCultureInfo("fi-FI"));
            this.Text = text;
        }
    }
}