using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var epearls = GetEpearls(html);
            foreach (var epearl in epearls)
            {
                Console.WriteLine(epearl); //– &#8211;
            }
        }
    }



    // <a href="https://summitlighthouse.org/ePearls-2011/2011-01-01-ePearl.pdf" target="_blank" rel="noopener">
    //1-1-2011 Gautama Buddha – An Experience Restored in the Third Eye and the Crown Chakra, Part 1</a>

    static List<UrlTitle> GetEpearls(string html)
    {
        var epearls = new List<UrlTitle>();

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
            var url = link.GetAttributeValue("href", string.Empty);
            epearls.Add(new UrlTitle(title, url));
        }
        return epearls;
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
