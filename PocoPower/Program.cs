using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.OrmLite;
using ServiceStack.Redis;
using ServiceStack.Text;

//Stand-alone Pre-Req: 
//Install-Package ServiceStack
//Install-Package ServiceStack.OrmLite.Sqlite.Mono

namespace PocoPower
{
    public class Config
    {
        public string GitHubName { get; set; }

        public string TwitterName { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var appSettings = new AppSettings();
            var config = appSettings.Get("my.config", new Config { GitHubName = "mythz", TwitterName = "ServiceStack" });

            var github = new GithubGateway();
            var repos = github.GetAllUserAndOrgsReposFor(config.GitHubName);

            var twitter = new TwitterGateway();
            var tweets = twitter.GetTimeline(config.TwitterName);

            "Loaded {0} repos and {1} tweets...".Print(repos.Count, tweets.Count);

            OrmLiteConfig.DialectProvider = SqliteDialect.Provider;
            //using (IDbConnection db = "~/../../db.sqlite".MapAbsolutePath().OpenDbConnection())
            using (IDbConnection db = ":memory:".OpenDbConnection())
            {
                db.DropAndCreateTable<Tweet>();
                db.DropAndCreateTable<GithubRepo>();

                "\nInserting {0} Tweets into Sqlite:".Print(tweets.Count);
                db.InsertAll(tweets);
                "\nLatest 5 Tweets from Sqlite:".Print();
                db.Select<Tweet>(q => q.OrderByDescending(x => x.id).Limit(5)).PrintDump();

                "\nInserting {0} Repos into Sqlite:".Print(repos.Count);
                db.InsertAll(repos);
                "\nLatest 5 Repos from Sqlite:".Print();
                db.Select<GithubRepo>(q => q.OrderByDescending(x => x.Id).Limit(5)).PrintDump();
            }

            using (var redis = new RedisClient())
            {
                "\nInserting {0} Tweets into Redis:".Print(tweets.Count);
                redis.StoreAll(tweets);
                "\n5 Tweets from Redis:".Print();
                redis.GetAll<Tweet>().Take(5).PrintDump();

                "\nInserting {0} Repos into Redis:".Print(repos.Count);
                redis.StoreAll(repos);
                "\n5 Repos from Redis:".Print();
                redis.GetAll<GithubRepo>().Take(5).PrintDump();
            }
        }
    }


    public class TwitterGateway
    {
        public const string UserTimelineUrl = "https://api.twitter.com/1.1/statuses/user_timeline.json?screen_name={0}";

        public List<Tweet> GetTimeline(string screenName, string sinceId = null, string maxId = null, int? take = null)
        {
            try
            {
                if (screenName == null)
                    throw new ArgumentNullException("screenName");

                var url = UserTimelineUrl.Fmt(screenName).AddQueryParam("count", take.GetValueOrDefault(100));
                if (!string.IsNullOrEmpty(sinceId))
                    url = url.AddQueryParam("since_id", sinceId);
                if (!string.IsNullOrEmpty(maxId))
                    url = url.AddQueryParam("max_id", maxId);

                var json = url.GetJsonFromUrl();
                var tweets = json.FromJson<List<Tweet>>();

                return tweets;

            }
            catch (Exception)
            {
                "Twitter no longer allows public feeds".Print();
                return new List<Tweet>();
            }
        }
    }

    public class Tweet
    {
        public ulong id { get; set; }
        public ulong? in_reply_to_status_id { get; set; }
        public bool retweeted { get; set; }
        public bool truncated { get; set; }
        public string created_at { get; set; }
        public ulong? in_reply_to_user_id { get; set; }
        public string in_reply_to_screen_name { get; set; }
        public TweetUser user { get; set; }
        public bool favorited { get; set; }
        public string source { get; set; }
        public string retweet_count { get; set; }
        public string text { get; set; }
        public GeoPoint geo { get; set; }
        public GeoPoint coordinates { get; set; }
    }

    public class TweetUser
    {
        public string name { get; set; }
        public string profile_sidebar_border_color { get; set; }
        public string profile_background_tile { get; set; }
        public string profile_sidebar_fill_color { get; set; }
        public string created_at { get; set; }
        public string profile_image_url { get; set; }
        public string profile_link_color { get; set; }
        public string location { get; set; }
        public string url { get; set; }
        public int favourites_count { get; set; }
        public bool contributors_enabled { get; set; }
        public string utc_offset { get; set; }
        public string id { get; set; }
        public string profile_use_background_image { get; set; }
        public string profile_text_color { get; set; }
        public bool @protected { get; set; }
        public int followers_count { get; set; }
        public string lang { get; set; }
        public bool verified { get; set; }
        public string profile_background_color { get; set; }
        public bool geo_enabled { get; set; }
        public bool? notifications { get; set; }
        public string description { get; set; }
        public string time_zone { get; set; }
        public int friends_count { get; set; }
        public int statuses_count { get; set; }
        public string profile_background_image_url { get; set; }
        public string screen_name { get; set; }
    }

    public class GeoPoint
    {
        public string type { get; set; }
        public double[] coordinates { get; set; }
    }

    public class GithubGateway
    {
        public const string GithubApiBaseUrl = "https://api.github.com/";

        public T GetJson<T>(string route, params object[] routeArgs)
        {
            return GithubApiBaseUrl.AppendPath(route.Fmt(routeArgs))
                .GetJsonFromUrl(req => req.UserAgent = "ServiceStack Poco/Power")
                .FromJson<T>();
        }

        public List<GithubOrg> GetUserOrgs(string githubUsername)
        {
            return GetJson<List<GithubOrg>>("users/{0}/orgs", githubUsername);
        }

        public List<GithubRepo> GetUserRepos(string githubUsername)
        {
            return GetJson<List<GithubRepo>>("users/{0}/repos", githubUsername);
        }

        public List<GithubRepo> GetOrgRepos(string githubOrgName)
        {
            return GetJson<List<GithubRepo>>("orgs/{0}/repos", githubOrgName);
        }

        public List<GithubRepo> GetAllUserAndOrgsReposFor(string githubUsername)
        {
            var map = new Dictionary<int, GithubRepo>();
            GetUserRepos(githubUsername).ForEach(x => map[x.Id] = x);
            GetUserOrgs(githubUsername).ForEach(org =>
                GetOrgRepos(org.Login)
                    .ForEach(repo => map[repo.Id] = repo));

            return map.Values.ToList();
        }
    }

    public class GithubRepo
    {
        public int Id { get; set; }
        public string Open_Issues { get; set; }
        public int Watchers { get; set; }
        public DateTime? Pushed_At { get; set; }
        public string Homepage { get; set; }
        public string Svn_Url { get; set; }
        public DateTime? Updated_At { get; set; }
        public string Mirror_Url { get; set; }
        public bool Has_Downloads { get; set; }
        public string Url { get; set; }
        public bool Has_issues { get; set; }
        public string Language { get; set; }
        public bool Fork { get; set; }
        public string Ssh_Url { get; set; }
        public string Html_Url { get; set; }
        public int Forks { get; set; }
        public string Clone_Url { get; set; }
        public int Size { get; set; }
        public string Git_Url { get; set; }
        public bool Private { get; set; }
        public DateTime Created_at { get; set; }
        public bool Has_Wiki { get; set; }
        public GithubUser Owner { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class GithubUser
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Avatar_Url { get; set; }
        public string Url { get; set; }
        public int? Followers { get; set; }
        public int? Following { get; set; }
        public string Type { get; set; }
        public int? Public_Gists { get; set; }
        public string Location { get; set; }
        public string Company { get; set; }
        public string Html_Url { get; set; }
        public int? Public_Repos { get; set; }
        public DateTime? Created_At { get; set; }
        public string Blog { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public bool? Hireable { get; set; }
        public string Gravatar_Id { get; set; }
        public string Bio { get; set; }
    }

    public class GithubOrg
    {
        public int Id { get; set; }
        public string Avatar_Url { get; set; }
        public string Url { get; set; }
        public string Login { get; set; }
    }
}
