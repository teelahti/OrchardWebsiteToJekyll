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
        private static readonly string[] SourceImageGalleries = { @"Media\Default\BlogPost\blog", @"Media\Default\Page", @"Media\Default\ImageGalleries" };

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: \r\n {0} sourcefolder targetfolder", 
                    Path.GetFileName(Assembly.GetEntryAssembly().Location));
                return;
            }

            var source = args[0];

            // Create necessary folders
            string baseFolder = args[1];
            string postsFolder = Path.Combine(baseFolder, "_posts");
            string imagesFolder = Path.Combine(baseFolder, "images");

            Directory.CreateDirectory(baseFolder);
            Directory.CreateDirectory(postsFolder);
            Directory.CreateDirectory(imagesFolder);

            // Use encoding that does not emit BOM, otherwise Jekyll will fail
            var encoding = new UTF8Encoding(false);
            var imageGalleriesHref = SourceImageGalleries.Select(s => s.Replace(@"\", "/")).ToArray();

            var comments = new DisqusBuilder();

            EnumerateFiles(source).AsParallel().ForAll(
                file =>
                {
                    var parsed = KsxParser.Parse(file, imageGalleriesHref);
                    var blogPost = parsed as KsxBlogPost;

                    if (blogPost != null)
                    {
                        File.WriteAllText(
                            Path.Combine(postsFolder, blogPost.NewSlug + ".md"),
                            blogPost.ToString(),
                            encoding);

                        comments.AddPost(blogPost);
                    }

                    var page = parsed as KsxPage;
                    if (page != null)
                    {
                        File.WriteAllText(
                            Path.Combine(baseFolder, page.Slug + ".md"),
                            page.ToString(),
                            encoding);
                    }

                    var problems = string.Join(Environment.NewLine, parsed.Problems);

                    Console.WriteLine(
                        "Converted: " + file.FileInfo.Name
                        + (string.IsNullOrEmpty(problems) ? "" : Environment.NewLine + problems));
                });

            // Output comments as Disqus XML format to be imported to Disqus 
            comments.Write(Path.Combine(baseFolder, "import-me-to-disqus.xml"));

            // Copy all image galleries
            var sourceGalleries = SourceImageGalleries
                .Select(d => Directory.EnumerateDirectories(Path.Combine(source, d)).ToList())
                .SelectMany(dlist => dlist);
                
            foreach (var imageGallery in sourceGalleries)
            {
                CopyDirectory(imageGallery, imagesFolder);
            }

            Console.WriteLine("\r\nDone, press ENTER to exit");
            Console.ReadLine();
        }

        private static void CopyDirectory(string source, string target)
        {
            var dir = new DirectoryInfo(source);
            var targetDirectory = Path.Combine(target, dir.Name);

            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            foreach (var f in dir.GetFiles())
            {
                f.CopyTo(Path.Combine(targetDirectory, f.Name), false);
            }

            // Recursively copy all subdirectories
            foreach (var subdirectory in dir.GetDirectories())
            {
                Program.CopyDirectory(subdirectory.FullName, targetDirectory);
            }
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
