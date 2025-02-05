using AspNet_Core_Grpc_Service;
using Grpc.Core;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("https://localhost:5279");

var client1 = new Notifier.NotifierClient(channel);
var request1 = new SubscriptionRequest { Topic = "Customer One" };
_ = Task.Run(() => ReadNotifications(client1, request1));

var client2 = new Notifier.NotifierClient(channel);
var request2 = new SubscriptionRequest { Topic = "Customer Two" };
_ = Task.Run(() => ReadNotifications(client2, request2));

Console.WriteLine("Registered customers. Waiting for notifications...");
Console.ReadKey();

static async Task ReadNotifications(Notifier.NotifierClient client, SubscriptionRequest request)
{
    try
    {
        using var call = client.Subscribe(request);
        await foreach (var notification in call.ResponseStream.ReadAllAsync())
        {
            Console.WriteLine($"Customer {client.GetHashCode()} - New notification for => {notification.Topic}: {notification.Message}");
        }
    }
    catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Cancelled)
    {
        Console.WriteLine($"Customer {client.GetHashCode()} - Registration canceled by the server");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Customer {client.GetHashCode()} - Error receiving notifications: {ex.Message}");
    }
    finally
    {
        Console.WriteLine($"Customer {client.GetHashCode()} - End of registration.");
    }
}