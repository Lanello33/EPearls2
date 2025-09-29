using HtmlAgilityPack;

namespace EPearls2;

internal class Downloader
{
    const string UrlPattern  = "https://summitlighthouse.org/{0}-epearls-archive/";
    const int StartYear   = 2011;
    const int EndYear     = 2025;

    record UrlTitle(string Url, string Title);

    public static void DownloadEpearls()
    {
        for (var year = StartYear; year <= EndYear; year++)
        {
            var yearUrl = string.Format(UrlPattern, year);
            var html = new HttpClient().GetStringAsync(yearUrl).Result;
            var urlTitles = GetUrlTitles(html);
            foreach (var urlTitle in urlTitles)
            {
                var epearl = FromUrlTitle(urlTitle);
                Console.WriteLine(epearl);
            }
        }
    }

    record Epearl(DateOnly Date, string Author, string Title, string Url);

    static Epearl FromUrlTitle(UrlTitle urlTitle)
    {
        //1-1-2011 Gautama Buddha &#8211; An Experience Restored in the Third Eye and the Crown Chakra, Part 1
        //1-1-2011 Gautama Buddha - An Experience Restored in the Third Eye and the Crown Chakra, Part 1

        var parts = urlTitle.Title.Split(' ', 2, StringSplitOptions.TrimEntries);
        var date = ParseDateOnly(parts[0]);
        if (date == DateOnly.MinValue)
        {
            if (parts[0].StartsWith("121-"))
            {
                date = ParseDateOnly(parts[0].Remove(2, 1));
            }
            throw new Exception($"Invalid date in title: {urlTitle.Title}");
        }
        var authorTitle = parts.Length > 1 ? parts[1] : string.Empty;
        var (author, title) = Parse(authorTitle);
        return new Epearl(date, author, title, urlTitle.Url);
    }

    static DateOnly ParseDateOnly(string text)
    {
        return DateOnly.TryParse(text, out DateOnly dateOnly) ? dateOnly : DateOnly.MinValue;
    }

    static (string author, string title) Parse(string authorTitle)
    {
        const string delimeter = "&#8211";
        const string minus = "-";
        const string emDash = "—"; // U+2014
        if (authorTitle.Contains(delimeter)) return Split(delimeter);
        if (authorTitle.Contains(minus)) return Split(minus);
        if (authorTitle.Contains(emDash)) return Split(emDash);

        return ("author", "");

        (string author, string title) Split(string delimeter)
        {
            var s = authorTitle.Split(delimeter, 2, StringSplitOptions.TrimEntries);
            return (s[0], s.Length > 1 ? s[1] : "?");
        }
    }

    // <a href="https://summitlighthouse.org/ePearls-2011/2011-01-01-ePearl.pdf" target="_blank" rel="noopener">
    //1-1-2011 Gautama Buddha – An Experience Restored in the Third Eye and the Crown Chakra, Part 1</a>

    static List<UrlTitle> GetUrlTitles(string html)
    {
        var urlTitles = new List<UrlTitle>();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var search = "https://summitlighthouse.org/ePearls-";
        var links = doc.DocumentNode.SelectNodes($"//a[contains(@href, '{search}')]");

        if (links == null)
        {
            throw new Exception("No links found");
        }

        foreach (var link in links)
        {
            // 1-1-2011 Gautama Buddha – An Experience Restored in the Third Eye and the Crown Chakra, Part 1
            var title = link.InnerText.Trim(); // 
            Console.WriteLine(title);
            var url = link.GetAttributeValue("href", string.Empty);
            urlTitles.Add(new UrlTitle(url, title));
        }
        return urlTitles;
    }


    static async Task DownloadPdfAsync(string url, string destinationPath)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync(destinationPath, content);
    }
}
