using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ServiceStack.Common.Web;
using ServiceStack.DataAnnotations;
using ServiceStack.Messaging;
using ServiceStack.OrmLite;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.ServiceInterface.ServiceModel;
using ServiceStack.Text;

namespace Reusability
{
    [Route("/smessage")]
    public class SMessage : IReturn<SMessageResponse>
    {
        public bool Defer { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class SMessageResponse
    {
        public long TimeTakenMs { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }

    [Route("/receipts")]
    public class FindReceipts
    {
        public int? Skip { get; set; }
        public int? Take { get; set; }
    }

    public class SMessageReceipt
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string RefId { get; set; }
        public string Type { get; set; }

        public string Error { get; set; }

        public string To { get; set; }
        public string From { get; set; }
    }

    public class SMessageService : Service
    {
        public EmailProvider Email { get; set; }
        public FacebookGateway Facebook { get; set; }
        public TwitterGateway Twitter { get; set; }

        public IMessageFactory MsgFactory { get; set; }

        public object Any(SMessage request)
        {
            var sw = Stopwatch.StartNew();

            if (!request.Defer)
            {
                var results = new List<SMessageReceipt>();
                results.AddRange(Email.Send(request));
                results.AddRange(Facebook.Send(request));
                results.AddRange(Twitter.Send(request));
                Db.InsertAll(results);
            }
            else
            {
                using (var producer = MsgFactory.CreateMessageProducer())
                {
                    Email.CreateMessages(request).ForEach(producer.Publish);
                    Facebook.CreateMessages(request).ForEach(producer.Publish);
                    Twitter.CreateMessages(request).ForEach(producer.Publish);
                }
            }

            return new SMessageResponse {
                TimeTakenMs = sw.ElapsedMilliseconds,
            };
        }

        public object Any(EmailMessage request)
        {
            var sw = Stopwatch.StartNew();
            Db.InsertAll(Email.Process(request));
            return new SMessageResponse { TimeTakenMs = sw.ElapsedMilliseconds };
        }

        public object Any(CallFacebook request)
        {
            var sw = Stopwatch.StartNew();
            Db.InsertAll(Facebook.Process(request));
            return new SMessageResponse { TimeTakenMs = sw.ElapsedMilliseconds };
        }

        public object Any(PostStatusTwitter request)
        {
            var sw = Stopwatch.StartNew();
            Db.InsertAll(Twitter.Process(request));
            return new SMessageResponse { TimeTakenMs = sw.ElapsedMilliseconds };
        }

        public object Any(FindReceipts request)
        {
            var skip = request.Skip.GetValueOrDefault(0);
            var take = request.Take.GetValueOrDefault(20);
            return Db.Select<SMessageReceipt>(q => q.Limit(skip, take));
        }
    }


    /* 
     * EMAIL
     */

    [Route("/email")]
    public class EmailMessage : IReturn<SMessageReceipt>
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class EmailProvider
    {
        public IDbConnectionFactory DbFactory { get; set; }

        public List<SMessageReceipt> Send(SMessage request)
        {
            return Process(CreateMessages(request).ToArray());
        }

        public List<EmailMessage> CreateMessages(SMessage request)
        {
            return DbFactory.Run(db => db.Select<EmailRegistration>()
                .ConvertAll(registration => new EmailMessage {
                    To = registration.Email,
                    Subject = request.Subject,
                    Body = request.Body,
                }));
        }

        public List<SMessageReceipt> Process(params EmailMessage[] messages)
        {
            var client = new JsonServiceClient("http://servicestackdemo.appspot.com");
            return messages.ToList().ConvertAll(client.Post);
        }
    }


    /* 
     * FACEBOOK
     */

    public class FacebookMessage : IReturn<SMessageReceipt>
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class CallFacebook
    {
        public CallFacebook()
        {
            this.Params = new Dictionary<string, string>();
        }
        public string UserName { get; set; }
        public string AuthToken { get; set; }
        public string Method { get; set; }
        public Dictionary<string, string> Params { get; set; }
    }

    public class FacebookGateway
    {
        private static int id;

        public IDbConnectionFactory DbFactory { get; set; }

        public List<SMessageReceipt> Send(SMessage request)
        {
            return Process(CreateMessages(request).ToArray());
        }

        static void Call(string method, string accessToken, Dictionary<string, string> args)
        {
            var url = "https://api.facebook.com/method/{0}?access_token={1}".Fmt(method, accessToken);
            foreach (var param in args)
                url = url.AddQueryParam(param.Key, param.Value);
            var response = url.GetStringFromUrl();
            response.Print();
        }

        public List<CallFacebook> CreateMessages(SMessage request)
        {
            return DbFactory.Run(db => db.Select<UserOAuthProvider>(q => q.Provider == "facebook")
                .ConvertAll(facebook => new CallFacebook {
                    UserName = facebook.UserName,
                    AuthToken = facebook.AccessTokenSecret,
                    Method = "users.setStatus",
                    Params = { { "status", request.Body } }
                }));
        }

        public List<SMessageReceipt> Process(params CallFacebook[] messages)
        {
            var results = new List<SMessageReceipt>();
            foreach (var message in messages)
            {
                Call(message.Method, message.AuthToken, message.Params);
                results.Add(new SMessageReceipt {
                    Type = "facebook",
                    To = message.UserName,
                    From = "Reusability App",
                    RefId = id++.ToString(),
                });
            }
            return results;
        }
    }


    /* 
     * TWITTER
     */

    public class PostStatusTwitter
    {
        public string ScreenName { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
        public string Status { get; set; }
    }

    public class TwitterGateway
    {
        private static int id;

        public IDbConnectionFactory DbFactory { get; set; }

        public List<SMessageReceipt> Send(SMessage request)
        {
            return Process(CreateMessages(request).ToArray());
        }

        public List<PostStatusTwitter> CreateMessages(SMessage request)
        {
            return DbFactory.Run(db => db.Select<UserOAuthProvider>(q => q.Provider == "twitter")
                .ConvertAll(twitter => new PostStatusTwitter {
                    ScreenName = twitter.UserName,
                    AccessToken = twitter.AccessToken,
                    AccessTokenSecret = twitter.AccessTokenSecret,
                    Status = request.Body,
                }));
        }

        public List<SMessageReceipt> Process(params PostStatusTwitter[] messages)
        {
            var results = new List<SMessageReceipt>();
            foreach (var message in messages)
            {
                var receipt = new SMessageReceipt {
                    Type = "twitter",
                    To = message.ScreenName,
                    From = "Reusability Twitter",
                    RefId = id++.ToString(),
                };
                try
                {
                    PostToUrl("http://api.twitter.com/1/statuses/update.json",
                        message.AccessToken, message.AccessTokenSecret,
                        new Dictionary<string, string> { { "status", message.Status } });
                }
                catch (Exception ex)
                {
                    receipt.Error = ex.Message;
                }

                results.Add(receipt);
            }
            return results;
        }

        public static string PostToUrl(string url, string accessToken, string accessTokenSecret, Dictionary<string, string> args, string acceptType = ContentType.Json)
        {
            var oAuthProvider = (OAuthProvider)AuthService.GetAuthProvider("twitter");
            var uri = new Uri(url);
            var webReq = (HttpWebRequest)WebRequest.Create(uri);
            webReq.Accept = acceptType;
            webReq.Method = HttpMethod.Post;

            string data = null;
            if (args != null)
            {
                var sb = new StringBuilder();
                foreach (var arg in args)
                {
                    if (sb.Length > 0)
                        sb.Append("&");
                    sb.AppendFormat("{0}={1}", arg.Key, OAuthUtils.PercentEncode(arg.Value));
                }
                data = sb.ToString();
            }

            webReq.Headers[HttpRequestHeader.Authorization] = OAuthAuthorizer.AuthorizeRequest(
                oAuthProvider, accessToken, accessTokenSecret, "POST", uri, data);

            if (data != null)
            {
                webReq.ContentType = ContentType.FormUrlEncoded;
                using (var writer = new StreamWriter(webReq.GetRequestStream()))
                    writer.Write(data);
            }

            using (var webRes = webReq.GetResponse())
                return webRes.DownloadText();
        }

    }

}