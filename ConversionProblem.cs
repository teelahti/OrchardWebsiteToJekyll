namespace KsxWebsiteToJekyll
{
    using System.Globalization;

    internal class ConversionProblem
    {
        public int LineNumber { get; set; }

        public string Line { get; set; }

        public ConversionProblem(int lineNumber, string line)
        {
            this.LineNumber = lineNumber;
            this.Line = line;
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture, "{0}: {1}", this.LineNumber, this.Line);
        }
    }
}