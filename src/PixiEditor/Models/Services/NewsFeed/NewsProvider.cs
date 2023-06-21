﻿using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using PixiEditor.Platform;
using PixiEditor.UpdateModule;

namespace PixiEditor.Models.Services.NewsFeed;

internal class NewsProvider
{
    private const string FeedUrl = "https://raw.githubusercontent.com/PixiEditor/news-feed/main/";
    public async Task<List<News>?> FetchNewsAsync()
    {
        List<News> allNews = new List<News>();
        await FetchFrom(allNews, "shared.json");
        await FetchFrom(allNews, $"{IPlatform.Current.Id}.json");

        var test = new News()
        {
            Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam euismod, " +
                    "nisl eget ultricies ultrices, nisl nisl ultricies nisl, nec",
            Date = DateTime.Now,
            ShortDescription =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam euismod, nisl eget ultricies ultrices, nisl nisl ultricies nisl, nec",
            NewsType = NewsType.BlogPost
        };

        allNews.Add(test);
        var test1 = new News()
        {
            Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam euismod, " +
                    "nisl eget ultricies ultrices, nisl nisl ultricies nisl, nec",
            Date = DateTime.Now,
            ShortDescription =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam euismod, nisl eget ultricies ultrices, nisl nisl ultricies nisl, nec",
            NewsType = NewsType.Misc
        };

        allNews.Add(test1);
        var test2 = new News()
        {
            Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam euismod, " +
                    "nisl eget ultricies ultrices, nisl nisl ultricies nisl, nec",
            Date = DateTime.Now,
            ShortDescription =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam euismod, nisl eget ultricies ultrices, nisl nisl ultricies nisl, nec",
            NewsType = NewsType.YtVideo
        };
        allNews.Add(test2);

        var test3 = new News()
        {
            Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam euismod, " +
                    "nisl eget ultricies ultrices, nisl nisl ultricies nisl, nec",
            Date = DateTime.Now,
            ShortDescription =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam euismod, nisl eget ultricies ultrices, nisl nisl ultricies nisl, nec",
            NewsType = NewsType.OfficialAnnouncement
        };
        allNews.Add(test3);
        var test4 = new News()
        {
            Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam euismod, " +
                    "nisl eget ultricies ultrices, nisl nisl ultricies nisl, nec",
            Date = DateTime.Now,
            ShortDescription =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam euismod, nisl eget ultricies ultrices, nisl nisl ultricies nisl, nec",
            NewsType = NewsType.NewVersion
        };

        allNews.Add(test4);

        return allNews.OrderByDescending(x => x.Date).Take(20).ToList();
    }

    private static async Task FetchFrom(List<News> output, string fileName)
    {
        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "PixiEditor");
        HttpResponseMessage response = await client.GetAsync($"{FeedUrl}{fileName}");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            string content = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<News>>(content);
            {
                output.AddRange(list);
            }
        }
    }
}