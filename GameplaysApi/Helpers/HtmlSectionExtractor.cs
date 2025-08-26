using HtmlAgilityPack;

public class HtmlSectionExtractor
{
    public string ExtractOverviewText(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Find first two <h2> nodes
        var firstH2 = doc.DocumentNode.SelectSingleNode("//h2");
        HtmlNode? secondH2 = null;
        if (firstH2 != null)
            secondH2 = firstH2.SelectSingleNode("following-sibling::h2");

        var content = "";

        if (firstH2 != null)
        {
            // Iterate siblings between <h2> nodes
            var currentNode = firstH2.NextSibling;
            while (currentNode != null && currentNode != secondH2)
            {
                // Only get text from element nodes that are not figure elements and skip empty text nodes
                if (currentNode.NodeType == HtmlNodeType.Element
                    && !currentNode.Name.Equals("figure", StringComparison.OrdinalIgnoreCase)
                    || (currentNode.NodeType == HtmlNodeType.Text && !string.IsNullOrWhiteSpace(currentNode.InnerText)))
                {
                    content += currentNode.InnerText.Trim() + " ";
                }
                currentNode = currentNode.NextSibling;
            }
        }

        if (string.IsNullOrEmpty(content))
            return "No description found.";

        return content.Trim();
    }
}
