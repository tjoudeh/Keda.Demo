using Azure.Messaging.ServiceBus;
using Bogus;
using Keda.Demo.Contracts;
using System.Text.Json;

namespace Keda.Demo.TrackingUpdatesGenerator
{
    internal class Program
    {
        private const string topicName = "trackingupdates";
        private const string connString = "Endpoint=sb://shipmentssvcbus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=M+Qs+y9Nr2yQMlDH83Vv7NREPkZGHftvZE/Siwz8ubE=";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Simulating shipments tracking updates, how many updates needed?");

            var requestedAmount = DetermineMsgsCount();
            await QueueShipments(requestedAmount);

            Console.WriteLine("Simulation completed, thank you.");
        }

        private static async Task QueueShipments(int messagesCount)
        {
            var serviceBusClient = new ServiceBusClient(connString);
            var serviceBusSender = serviceBusClient.CreateSender(topicName);

            for (int currentCount = 0; currentCount < messagesCount; currentCount++)
            {
                var shipment = GenerateShipment();
                var rawShipment =  JsonSerializer.Serialize(shipment);
                var shipmentMessage = new ServiceBusMessage(rawShipment);

                Console.WriteLine($"Queuing tracking update for Shipment id '{shipment.ShipmentId}' and Waybill '{shipment.WaybillNo}' - Recipient '{shipment.Recipient.Name}' updates on address {shipment.Recipient.NotificationAddress}");
                await serviceBusSender.SendMessageAsync(shipmentMessage);
            }
        }

        private static Shipment GenerateShipment()
        {
            var trackingCodes = new[] { "DEL", "OFD", "TRS", "HLD" };

            var recipientGenerator = new Faker<Recipient>()
                .RuleFor(u => u.Name, (f, u) => f.Name.FullName())
                .RuleFor(u => u.NotficationChannel, (f, u) => f.Random.Byte(1,2)) //1: Email, 2: SMS
                .RuleFor(u => u.NotificationAddress, (f, u) => u.NotficationChannel.Equals(1)? f.Internet.Email() : f.Random.ULong(009627950000, 00962799999).ToString());

            var trackingUpdateGenerator = new Faker<TrackingUpdate>()
            .RuleFor(u => u.Code, (f, u) => f.PickRandom(trackingCodes))
            .RuleFor(u => u.Source, (f, u) => $"{f.Address.CountryCode()}, {f.Address.City()}")
            .RuleFor(u => u.Destination, (f, u) => f.Address.FullAddress())
            .RuleFor(u => u.Description, (f, u) => f.Random.Words(3))
            .RuleFor(u => u.TimeStamp, (f, u) => DateTime.UtcNow.AddMinutes(f.Random.Int(-60,0)));

            var shipmentGenerator = new Faker<Shipment>()
                .RuleFor(u => u.Recipient, () => recipientGenerator)
                .RuleFor(u => u.TrackingUpdate, () => trackingUpdateGenerator)
                .RuleFor(u => u.ShipmentId, f => f.Random.Uuid())
                .RuleFor(u => u.WaybillNo, f => f.Random.ULong(100000000, 999999999).ToString())
                .RuleFor(u => u.CreatedOn, f => f.Date.Between(DateTime.UtcNow.AddDays(-5).Date, DateTime.UtcNow.AddDays(-1).Date));

            return shipmentGenerator.Generate();
        }

        private static int DetermineMsgsCount()
        {
            var rawAmount = Console.ReadLine();
            if (int.TryParse(rawAmount, out int amount))
            {
                return amount;
            }

            Console.WriteLine("Not a valid number, please try again");
            return DetermineMsgsCount();
        }
    }
}