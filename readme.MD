# Convert Orchard-based web site to Jekyll markdown #

This is a one time converting tool used to migrate old Orchard CMS -based [kerkesix.fi web site](http://kerkesix.fi) to Markdown-based Jekyll blog. Internally uses Pandoc for the conversion. 

The procedure used was: 

1. Copy the whole web site to local folder with [wget](http://en.wikipedia.org/wiki/Wget)
2. Run this tool against the local copy. Assumes Pandoc is installed to the default location. 
3. Copy outcome folders to Jekyll blog (posts to _posts folder etc.)
4. Do some manual corrections as there are some HTML that [pandoc](http://johnmacfarlane.net/pandoc/) couldn't convert. 
