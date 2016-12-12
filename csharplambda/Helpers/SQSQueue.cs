using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;

namespace csharplambda.Helpers
{
    public class SQSQueue : IDisposable
    {
        public static AmazonSQSConfig config = new AmazonSQSConfig() { RegionEndpoint = Amazon.RegionEndpoint.APSoutheast2 };

        private readonly Message M;
        private readonly string streamName;

        private static BasicAWSCredentials creds = new BasicAWSCredentials("public",
            "private");

        private SQSQueue() { }

        private SQSQueue(Message m, string streamName)
        {
            M = m;
            this.streamName = streamName;
        }

        public static async Task Clear(string streamName)
        {
            var client = new AmazonSQSClient(creds, config);
            await client.PurgeQueueAsync(streamName);
        }

        public static async Task<SQSQueue> Read(string streamName)
        {
            var client = new AmazonSQSClient(creds, config);

            var res = await client.ReceiveMessageAsync(streamName);
            if (res.Messages.Any())
            {
                var m1 = res.Messages[0];

                return new SQSQueue(m1, streamName);
            }
            return null;
        }

        public T GetData<T>() where T : class
        {
            {
                if (M == null)
                    return null;

                var body = M.Body;
                if (typeof(T) == typeof(string))
                    return (T)(object)body;

                var ret = JsonConvert.DeserializeObject<T>(body);
                return ret;
            }
        }

        public async Task Accept()
        {
            if (M == null)
                return;

            var client = new AmazonSQSClient(creds, config);
            await client.DeleteMessageAsync(streamName, M.ReceiptHandle);
        }

        public static async Task<bool> Write<T>(string streamName, T item)
        { return await Write(streamName, new List<T>() { item }); }

        public static async Task<bool> Write<T>(string streamName, List<T> list)
        {
            const int writecount = 10;
            int c = 0;
            while (c < list.Count)
            {
                var vals = list.Skip(c).Take(writecount).ToList();
                var ok = await WriteSplit(streamName, vals);
                if (!ok)
                    return false;
                c += writecount;
            }
            return true;
        }

        private static async Task<bool> WriteSplit<T>(string streamName, List<T> list)
        {
            var client = new AmazonSQSClient(creds, config);

            var data = new List<SendMessageBatchRequestEntry>();
            foreach (var l2 in list)
            {
                var json = JsonConvert.SerializeObject(l2);
                data.Add(new SendMessageBatchRequestEntry(data.Count.ToString(), json));
            }

            var res = await client.SendMessageBatchAsync(new SendMessageBatchRequest()
            {
                Entries = data,
                QueueUrl = streamName
            });
            return !res.Failed.Any();
        }

        public void Dispose()
        {
            var t = Accept();
            t.Wait();
        }
    }
}