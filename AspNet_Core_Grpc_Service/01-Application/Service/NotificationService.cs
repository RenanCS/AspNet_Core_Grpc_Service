using AspNet_Core_Grpc_Service._02_Domain.Interface;
using Grpc.Core;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace AspNet_Core_Grpc_Service._01_Application.Service
{
    public class NotificationService : INotificationService
    {
        private readonly ConcurrentDictionary<string, Channel<Notification>> _subscribers;

        public NotificationService(ConcurrentDictionary<string, Channel<Notification>> subscribers)
        {
            _subscribers = subscribers;
        }

        public NotificationService() : this(new ConcurrentDictionary<string, Channel<Notification>>()) { }


        public async Task RegistrySubscription(SubscriptionRequest request, IServerStreamWriter<Notification> responseStream, ServerCallContext context)
        {
            var channel = _subscribers.GetOrAdd(request.Topic, _ => Channel.CreateUnbounded<Notification>());

            // Write existing notifications to the customer (if any)
            while (channel.Reader.TryRead(out var existingNotification))
            {
                await responseStream.WriteAsync(existingNotification);
            }

            try
            {
                while (await channel.Reader.WaitToReadAsync(context.CancellationToken))
                {
                    if (channel.Reader.TryRead(out var notification))
                    {
                        await responseStream.WriteAsync(notification);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Customer disconnected, record log 
            }
            finally
            {
                _subscribers.TryRemove(request.Topic, out _);
            }
        }

        public async Task PublishNotificationForAll(string message)
        {
            var tasks = new List<Task>();

            foreach (var topic in _subscribers.Keys.ToList())
            {
                if (_subscribers.TryGetValue(topic, out var channel))
                {
                    var messageCopy = string.Copy(message);

                    tasks.Add(Task.Run(async () =>
                    {
                        await channel.Writer.WriteAsync(new Notification { Topic = topic, Message = messageCopy });
                    }));

                }
            }
            await Task.WhenAll(tasks);
        }

        public async Task PublishNotificationOne(string topic, string message)
        {
            if (_subscribers.TryGetValue(topic, out var channel))
            {
                await channel.Writer.WriteAsync(new Notification { Topic = topic, Message = message });
            }
        }
    }
}
