using System;
using System.Globalization;
using ServiceStack.Redis;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceHost;

namespace Reusability
{
    public class SMessage : IReturn<SMessageReceipt>
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class SMessageReceipt
    {
        public string Id { get; set; }
        public string Notes { get; set; }

        public string To { get; set; }
        public string From { get; set; }
    }

    public class SMessageService : IService, IDisposable
    {
        public IRedisClientsManager RedisManager { get; set; }

        public object Any(SMessage request)
        {
            var sMessages = Redis.As<SMessage>().Lists["sent:SMessage"];
            sMessages.Add(request);

            var client = new JsonServiceClient("http://servicestackdemo.appspot.com");
            var mailReceipt = client.Post(request);

            var receipt = new SMessageReceipt {
                Id = Redis.As<SMessageReceipt>().GetNextSequence().ToString(),
                To = request.To,
                From = mailReceipt.From,
            };

            return receipt;
        }


        IRedisClient redis;
        IRedisClient Redis
        {
            get { return redis ?? (redis = RedisManager.GetClient()); }
        }

        public void Dispose()
        {
            if (redis != null)
                redis.Dispose();
        }
    }
}