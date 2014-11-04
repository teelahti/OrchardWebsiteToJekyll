namespace KsxWebsiteToJekyll
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: \r\n {0} sourcefolder targetfolder", 
                    Path.GetFileName(Assembly.GetEntryAssembly().Location));
                return;
            }

            // Create necessary folders
            string baseFolder = args[1];
            string postsFolder = Path.Combine(baseFolder, "_posts");
            string imagesFolder = Path.Combine(baseFolder, "images");

            Directory.CreateDirectory(baseFolder);
            Directory.CreateDirectory(postsFolder);
            Directory.CreateDirectory(imagesFolder);

            // Use encoding that does not emit BOM, otherwise Jekyll will fail
            var encoding = new UTF8Encoding(false);

            EnumerateFiles(args[0]).AsParallel().ForAll(
                file =>
                {
                    var parsed = KsxParser.Parse(file);
                    var blogPost = parsed as KsxBlogPost;

                    if (blogPost != null)
                    {
                        File.WriteAllText(
                            Path.Combine(postsFolder, blogPost.NewSlug + ".md"), 
                            blogPost.ToString(),
                            encoding);
                    }

                    var page = parsed as KsxPage;
                    if (page != null)
                    {
                        File.WriteAllText(
                            Path.Combine(baseFolder, page.Slug + ".md"), 
                            page.ToString(),
                            encoding);
                    }

                    // TODO: Copy images
                    // TODO: Output comments.xml for disqus use        
                    Console.WriteLine("Converted: " + file.FileInfo.Name);
                });

            Console.WriteLine("\r\nDone, press ENTER to exit");
            Console.ReadLine();
        }

        static IEnumerable<SourceDocument> EnumerateFiles(string folder)
        {
            return
                Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly)
                    .Select(f => new SourceDocument(f))
                    .Where(d => d.Document != null);
        }
    }
}
