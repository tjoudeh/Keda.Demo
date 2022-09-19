using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace Keda.Demo.TrackingUpdatesProcessor.BackgroundServices
{
    public abstract class TrackingUpdatesService<TMessage> : BackgroundService
    {

        protected readonly IConfiguration _configuration;
        protected readonly ILogger<TrackingUpdatesService<TMessage>> _logger;

        public TrackingUpdatesService(IConfiguration configuration, ILogger<TrackingUpdatesService<TMessage>> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var topicName = _configuration.GetValue<string>("ServiceBus:TopicName");
            var subscriptionName = _configuration.GetValue<string>("ServiceBus:SubscriptionName");

            var messageProcessor = BuildServiceBusProcessor(topicName, subscriptionName);
            messageProcessor.ProcessMessageAsync += HandleMessageAsync;
            messageProcessor.ProcessErrorAsync += HandleReceivedExceptionAsync;

            _logger.LogInformation($"Starting message pump on queue {topicName} in namespace {messageProcessor.FullyQualifiedNamespace}");
            await messageProcessor.StartProcessingAsync(stoppingToken);
            _logger.LogInformation("Message pump started");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            _logger.LogInformation("Closing message listner");
            await messageProcessor.CloseAsync(cancellationToken: stoppingToken);
            _logger.LogInformation("Message listner closed at: {ts}", DateTimeOffset.UtcNow);
        }

        private ServiceBusProcessor BuildServiceBusProcessor(string topicName, string subscriptionName)
        {

            var connectionString = _configuration.GetValue<string>("ServiceBus:ConnectionString");
            var serviceBusClient =  new ServiceBusClient(connectionString);

            var messageProcessor = serviceBusClient.CreateProcessor(topicName, subscriptionName);
            return messageProcessor;
        }


        private async Task HandleMessageAsync(ProcessMessageEventArgs processMessageEventArgs)
        {
            try
            {
                var rawMessageBody = processMessageEventArgs.Message.Body.ToString();

                _logger.LogInformation("Received message {MessageId} with body {MessageBody}",
                    processMessageEventArgs.Message.MessageId, rawMessageBody);

                var shipment = JsonSerializer.Deserialize<TMessage>(rawMessageBody);
                if (shipment != null)
                {
                    await ProcessMessage(shipment, processMessageEventArgs.Message.MessageId,
                        processMessageEventArgs.Message.ApplicationProperties,
                        processMessageEventArgs.CancellationToken);
                }
                else
                {
                    _logger.LogError(
                        "Unable to deserialize to message contract {ContractName} for message {MessageBody}",
                        typeof(TMessage), rawMessageBody);
                }

                _logger.LogInformation("Message {MessageId} processed", processMessageEventArgs.Message.MessageId);

                await processMessageEventArgs.CompleteMessageAsync(processMessageEventArgs.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to handle message");
            }
        }

        private Task HandleReceivedExceptionAsync(ProcessErrorEventArgs exceptionEvent)
        {
            _logger.LogError(exceptionEvent.Exception, "Unable to process message");
            return Task.CompletedTask;
        }

        protected abstract Task ProcessMessage(TMessage shipment,
                                            string messageId,
                                            IReadOnlyDictionary<string, object> userProperties,
                                            CancellationToken cancellationToken);
    }
}
