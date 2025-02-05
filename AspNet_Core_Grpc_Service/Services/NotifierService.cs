using AspNet_Core_Grpc_Service._02_Domain.Interface;
using Grpc.Core;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace AspNet_Core_Grpc_Service.Services
{
    public class NotifierService : Notifier.NotifierBase
    {
        private readonly ConcurrentDictionary<string, Channel<Notification>> _subscribers = new();

        private readonly INotificationService _notificationService;
        public NotifierService(INotificationService notificationService)
        {
            _notificationService = notificationService;

            Testscheduling();
        }
        public override async Task Subscribe(SubscriptionRequest request, IServerStreamWriter<Notification> responseStream, ServerCallContext context)
        {
            await _notificationService.RegistrySubscription(request, responseStream, context);
        }

        // Usage example:
        public async Task Testscheduling()
        {
            TimeSpan delay = TimeSpan.FromMinutes(1);

            Console.WriteLine($"Notification scheduled for {delay.TotalMinutes} minute(s).");

            await ScheduleNotification("Thank you for your registration!", delay);

            Console.WriteLine("Schedule completed.");
        }

        public async Task ScheduleNotification(string message, TimeSpan delay)
        {
            await Task.Delay(delay);

            await _notificationService.PublishNotificationForAll(message);
        }

    }
}
