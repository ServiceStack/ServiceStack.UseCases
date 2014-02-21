using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ServiceStack;
using ServiceStack.Messaging;
using ServiceStack.Redis;
using ServiceStack.Text;

namespace Reusability
{
    [Route("/mqstats")]
    public class MqStats
    {
        public bool Stop { get; set; }
        public bool Start { get; set; }
    }

    public class MqStatsResponse
    {
        public List<string> ActiveMqServices { get; set; }
        public string MqServiceStatus { get; set; }
        public object MqServiceStats { get; set; }

        public long InQ { get; set; }
        public long PriorityQ { get; set; }
        public long OutQ { get; set; }
        public long DlQ { get; set; }

        public List<MessageStat> Results { get; set; }

        public MessageHandlerStats MessageStats { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class MessageStat
    {
        public string MqName { get; set; }
        public string MqType { get; set; }
        public string MessageType { get; set; }
        public long Count { get; set; }
        public DateTime? FirstPublishedAt { get; set; }
        public DateTime? LastPublishedAt { get; set; }
        public string Error { get; set; }

        public string Oldest
        {
            get
            {
                return LastPublishedAt != null
                    ? (DateTime.UtcNow - LastPublishedAt).ToString()
                    : null;
            }
        }
    }

    public class MessageHandlerStats
    {
        public int Processed { get; set; }
        public int Failed { get; set; }
        public int Retries { get; set; }
        public int InQReceived { get; set; }
        public int PriorityQReceived { get; set; }

        public MessageHandlerStats() { }
        public MessageHandlerStats(IMessageHandlerStats stats)
        {
            Processed = stats.TotalMessagesProcessed;
            Failed = stats.TotalMessagesFailed;
            Retries = stats.TotalRetries;
            InQReceived = stats.TotalNormalMessagesReceived;
            PriorityQReceived = stats.TotalPriorityMessagesReceived;
        }
    }

    public class RedisMqServerStatsService : Service
    {
        public IRedisClientsManager RedisManager { get; set; }
        public IMessageService MessagesService { get; set; }

        public object Any(MqStats request)
        {
            using (var redis = RedisManager.GetClient())
            {
                if (request.Stop)
                    MessagesService.Stop();
                if (request.Start)
                    MessagesService.Start();

                var redisMqStats = new List<MessageStat>();

                var keys = redis.SearchKeys("mq:*");
                foreach (var key in keys)
                {
                    if (redis.GetEntryType(key) != RedisKeyType.List) continue;
                    var mqKeyParts = key.Split(new[] { ':', '.' });
                    if (mqKeyParts.Length != 3) continue;

                    var stat = new MessageStat
                    {
                        MqName = key,
                        MqType = mqKeyParts[2],
                        MessageType = mqKeyParts[1],
                        Count = redis.GetListCount(key),
                    };

                    var jsonFirstMsg = redis.GetItemFromList(key, 0);
                    try
                    {
                        var firstMsg = !string.IsNullOrEmpty(jsonFirstMsg) ? jsonFirstMsg.FromJson<IMessage>() : null;
                        stat.FirstPublishedAt = firstMsg != null
                            ? firstMsg.CreatedDate
                            : (DateTime?)null;
                    }
                    catch (Exception ex)
                    {
                        stat.Error = ex.Message + ", FirstMsg: " + jsonFirstMsg.SafeSubstring(0, 100) + "...";
                    }

                    var jsonLastMsg = redis.GetItemFromList(key, -1);
                    try
                    {
                        var lastMessage = !string.IsNullOrEmpty(jsonLastMsg) ? jsonLastMsg.FromJson<IMessage>() : null;
                        stat.LastPublishedAt = lastMessage != null
                            ? lastMessage.CreatedDate
                            : (DateTime?)null;
                    }
                    catch (Exception ex)
                    {
                        stat.Error = ex.Message + ", LastMsg: " + jsonLastMsg.SafeSubstring(0, 100) + "...";
                    }

                    redisMqStats.Add(stat);
                }

                var activeMqs = new HashSet<string>(MessagesService.RegisteredTypes.Select(x => x.Name), StringComparer.InvariantCultureIgnoreCase);
                var response = new MqStatsResponse
                {
                    ActiveMqServices = activeMqs.OrderBy(x => x).ToList(),
                    MqServiceStatus = MessagesService.GetStatus(),

                    InQ = redisMqStats.Where(x => x.MqType == "inq" && activeMqs.Contains(x.MessageType)).Sum(x => x.Count),
                    PriorityQ = redisMqStats.Where(x => x.MqType == "priorityq" && activeMqs.Contains(x.MessageType)).Sum(x => x.Count),
                    OutQ = redisMqStats.Where(x => x.MqType == "outq" && activeMqs.Contains(x.MessageType)).Sum(x => x.Count),
                    DlQ = redisMqStats.Where(x => x.MqType == "dlq" && activeMqs.Contains(x.MessageType)).Sum(x => x.Count),
                    Results = redisMqStats.Where(x => (x.MqType == "inq" || x.MqType == "priorityq") && activeMqs.Contains(x.MessageType)).ToList(),

                    MessageStats = new MessageHandlerStats(MessagesService.GetStats()),
                };

                return response;
            }
        }

        public const int DefaultTakeCount = 100;

        public object Any(MqDump request)
        {
            using (var Redis = RedisManager.GetClient())
            {
                var dtoType = request.Type == typeof(CallFacebook).Name
                    ? typeof(CallFacebook) :
                      //request.Type == typeof(PostStatusTwitter).Name
                      //? typeof(PostStatusTwitter) :
                      request.Type == typeof(EmailMessage).Name
                    ? typeof(EmailMessage) :
                      typeof(PostStatusTwitter);

                var queueNames = new QueueNames(dtoType);

                var startingFrom = request.Skip.GetValueOrDefault(0);
                var endingAt = startingFrom + request.Take.GetValueOrDefault(DefaultTakeCount) - 1;

                var priorityqList = Redis.Lists[queueNames.Priority];
                var priorityqMsgs = priorityqList.GetRange(startingFrom, endingAt).ConvertAll(x => CreateMessage(dtoType, x));

                var inqList = Redis.Lists[queueNames.In];
                var inqMsgs = inqList.GetRange(startingFrom, endingAt).ConvertAll(x => CreateMessage(dtoType, x));

                var outqList = Redis.Lists[queueNames.Out];
                var outqMsgs = outqList.GetRange(startingFrom, endingAt).ConvertAll(x => CreateMessage(dtoType, x));

                var dlqList = Redis.Lists[queueNames.Dlq];
                var dlqMsgs = dlqList.GetRange(startingFrom, endingAt).ConvertAll(x => CreateMessage(dtoType, x));

                var response = new MqDumpResponse
                {
                    PriorityQCount = priorityqList.Count,
                    PriorityQEmails = priorityqMsgs,
                    InQCount = inqList.Count,
                    InQEmails = inqMsgs,
                    OutQCount = outqList.Count,
                    OutQEmails = outqMsgs,
                    DlQCount = dlqList.Count,
                    DlQEmails = dlqMsgs,
                };

                return response;
            }
        }

        public static ConcurrentDictionary<Type, MessageFactoryDelegate> CtorMap = new ConcurrentDictionary<Type, MessageFactoryDelegate>();

        public static Message CreateMessage(Type type, string fromJson)
        {
            var fn = CtorMap.GetOrAdd(type, (t) =>
            {
                var genericType = typeof(MessageFactory<>).MakeGenericType(t);
                var mi = genericType.GetMethod("FromJson", BindingFlags.Public | BindingFlags.Static);
                var factoryfn = (MessageFactoryDelegate)Delegate.CreateDelegate(typeof(MessageFactoryDelegate), mi);
                return factoryfn;
            });

            return fn(fromJson);
        }
    }

    public delegate Message MessageFactoryDelegate(string json);

    public class MessageFactory<T>
    {
        public static Message FromJson(string json)
        {
            if (json.IsNullOrEmpty()) return null;
            var message = JsonSerializer.DeserializeFromString<Message>(json);
            return message;
        }
    }


    [Route("/mqdump")]
    [Route("/mqdump/{Type}")]
    public class MqDump
    {
        public string Type { get; set; }

        public int? Skip { get; set; }

        public int? Take { get; set; }
    }

    public class MqDumpResponse
    {
        public MqDumpResponse()
        {
            this.InQEmails = new List<Message>();
        }

        public int PriorityQCount { get; set; }
        public List<Message> PriorityQEmails { get; set; }
        public int InQCount { get; set; }
        public List<Message> InQEmails { get; set; }
        public int OutQCount { get; set; }
        public List<Message> OutQEmails { get; set; }
        public int DlQCount { get; set; }
        public List<Message> DlQEmails { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
    }

}