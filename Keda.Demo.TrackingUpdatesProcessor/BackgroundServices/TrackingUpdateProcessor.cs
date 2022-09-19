using Keda.Demo.Contracts;


namespace Keda.Demo.TrackingUpdatesProcessor.BackgroundServices
{
    public class TrackingUpdateProcessor : TrackingUpdatesService<Shipment>
    {
        public TrackingUpdateProcessor(IConfiguration configuration, ILogger<TrackingUpdatesService<Shipment>> logger) : base(configuration, logger)
        {
        }

        protected override async Task ProcessMessage(Shipment shipment,
                                                    string messageId, IReadOnlyDictionary<string, object> userProperties, CancellationToken cancellationToken)
        {

            _logger.LogInformation("Processing Tracking Updates for shipment '{ShipmentId}' with WaybillNo '{WaybillNo}' " +
                                     "Tracking Code '{Code}' update will be sent to Recipient '{recipientName}' on '{notificationAddress}'",
                                                    shipment.ShipmentId, shipment.WaybillNo, 
                                                    shipment.TrackingUpdate.Code, 
                                                    shipment.Recipient.Name, 
                                                    shipment.Recipient.NotificationAddress);

            //Simlate actual sending for tracking update
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

            _logger.LogInformation("Tracking Updates for shipment {ShipmentId} processed successfully", shipment.ShipmentId);
        }

    }
}