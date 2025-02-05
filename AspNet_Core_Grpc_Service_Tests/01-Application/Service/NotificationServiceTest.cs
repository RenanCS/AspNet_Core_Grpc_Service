using AspNet_Core_Grpc_Service;
using AspNet_Core_Grpc_Service._01_Application.Service;
using Grpc.Core;
using Moq;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace AspNet_Core_Grpc_Service_Tests
{
    public class NotificationServiceTest
    {

        [Fact]
        public async Task Should_ReceiveNotification_When_Available()
        {
            // Arrange
            var topic = "meu_topico";
            var notification = new Notification { Topic = topic, Message = "Mensage Test" };
            var testChannel = Channel.CreateUnbounded<Notification>();
            var subscribers = new ConcurrentDictionary<string, Channel<Notification>>();
            subscribers.TryAdd(topic, testChannel);

            var mockResponseStream = new Mock<IServerStreamWriter<Notification>>();
            var mockContext = new Mock<ServerCallContext>();
            var request = new SubscriptionRequest { Topic = topic };
            var service = new NotificationService(subscribers);

            var notificationReceived = new SemaphoreSlim(0);

            var subscriptionTask = Task.Run(async () =>
            {
                await service.RegistrySubscription(request, mockResponseStream.Object, mockContext.Object);
            });

            mockResponseStream.Setup(x => x.WriteAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
                .Callback(() => notificationReceived.Release());

            await Task.Delay(50);

            // Act
            await testChannel.Writer.WriteAsync(notification);

            await notificationReceived.WaitAsync(TimeSpan.FromSeconds(5));

            // Assert
            Assert.True(subscribers.ContainsKey(topic));

            mockContext.Setup(x => x.CancellationToken).Returns(new CancellationToken(true));

            await subscriptionTask;
        }
    }
}