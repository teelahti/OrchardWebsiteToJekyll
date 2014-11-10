namespace KsxWebsiteToJekyll
{
    using System;
    using System.IO;
    using System.Linq;

    using System.Collections.Generic;
    using System.Text;

    internal class DisqusBuilder
    {
        private const string ItemsPlaceholderString = "$items$";
        private const string CommentsPlaceholderString = "$comments$";
        private const string DisqusDateFormat = "yyyy-MM-dd HH:mm:ss";

        private readonly string disqusTemplate;
        private readonly string disqusItemTemplate;
        private readonly string disqusCommentTemplate;
        private readonly List<KsxBlogPost> posts = new List<KsxBlogPost>();

        public DisqusBuilder()
        {
            // Very hacky templating, but works in one-time setup like this
            disqusTemplate = File.ReadAllText("DisqusImportTemplate.xml");

            var itemStart = disqusTemplate.IndexOf("<item>", StringComparison.OrdinalIgnoreCase);
            var itemEnd = disqusTemplate.IndexOf("</item>", StringComparison.OrdinalIgnoreCase) + 7;

            disqusItemTemplate = disqusTemplate.Substring(itemStart, itemEnd - itemStart);

            // Replace the item from the main template with placeholder
            disqusTemplate = disqusTemplate.Substring(0, itemStart) + ItemsPlaceholderString + disqusTemplate.Substring(itemEnd);

            var commentStart = disqusItemTemplate.IndexOf("<wp:comment>", StringComparison.OrdinalIgnoreCase);
            var commentEnd = disqusItemTemplate.IndexOf("</wp:comment>", StringComparison.OrdinalIgnoreCase) + 13;

            disqusCommentTemplate = disqusItemTemplate.Substring(commentStart, commentEnd - commentStart);  

            // Replace the comment from the item template with placeholder
            disqusItemTemplate = disqusItemTemplate.Substring(0, commentStart) + CommentsPlaceholderString + disqusItemTemplate.Substring(commentEnd);
        }

        public void AddPost(KsxBlogPost post)
        {
            if (post.Comments.Any())
            {
                this.posts.Add(post);
            }
        }

        public void Write(string path)
        {
            var encodingWithoutBom = new UTF8Encoding(false);
            File.WriteAllText(path, ToString(), encodingWithoutBom);
        }

        public override string ToString()
        {
            var items = new StringWriter();

            foreach (var post in this.posts)
            {
                items.WriteLine(this.RenderItem(post));
            }

            return this.disqusTemplate.Replace(ItemsPlaceholderString, items.ToString());
        }

        private string RenderItem(KsxBlogPost post)
        {
            var comments = post.Comments.Aggregate(string.Empty, (current, s) => current + (this.RenderComment(s) + "\r\n"));

            var item = this.disqusItemTemplate
                .Replace("$article-title$", post.Title)
                .Replace("$article-slug$", post.Slug)
                .Replace("$article-date$", post.Date.ToString(DisqusDateFormat));

            return item.Replace(CommentsPlaceholderString, comments);
        }

        private string RenderComment(BlogComment blogComment)
        {
            return this.disqusCommentTemplate
                .Replace("$comment-id$", Guid.NewGuid().ToString())
                .Replace("$comment-author$", blogComment.Who)
                .Replace("$comment-date$", blogComment.When.ToString(DisqusDateFormat))
                .Replace("$comment-text", blogComment.Text);
        }
    }
}