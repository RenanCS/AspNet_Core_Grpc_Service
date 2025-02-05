using Grpc.Core;

namespace AspNet_Core_Grpc_Service._02_Domain.Interface
{
    public interface INotificationService
    {
        Task RegistrySubscription(SubscriptionRequest request, IServerStreamWriter<Notification> responseStream, ServerCallContext context);
        Task PublishNotificationForAll(string message);
        Task PublishNotificationOne(string topic, string message);
    }
}
