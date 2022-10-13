using Keda.Demo.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Keda.Demo.TrackingUpdatesProcessor.Controllers
{
    [Route("api/updatesprocessor")]
    [ApiController]
    public class UpdatesProcessorController : ControllerBase
    {
        private readonly ILogger _logger;

        public UpdatesProcessorController(ILogger<UpdatesProcessorController> logger)
        {
            _logger = logger;
        }

        [Dapr.Topic("pubsub-servicebus", "trackingupdates")]
        [HttpPost("TrackingUpdateReceived")]
        public async Task<IActionResult> TrackingUpdateReceived([FromBody] Shipment shipment)
        {
            _logger.LogInformation("Processing Tracking Updates for shipment '{ShipmentId}' with WaybillNo '{WaybillNo}' " +
                                     "Tracking Code '{Code}' update will be sent to Recipient '{recipientName}' on '{notificationAddress}'",
                                                    shipment.ShipmentId, shipment.WaybillNo,
                                                    shipment.TrackingUpdate.Code,
                                                    shipment.Recipient.Name,
                                                    shipment.Recipient.NotificationAddress);

            var sendResult = await SendTrackingUpdateNotification(shipment);

            if (sendResult)
            {
                // HTTP Status 2xx Message is processed as per status in payload (SUCCESS if empty; ignored if invalid payload).
                return Ok();
            }

            // HTTP Status 400 or 5xx Warning is logged and message to be retried based on service broker configuration
            return BadRequest($"Failed to send Tracking Update Notification");

            // HTTP Status 404 Error is logged and message is dropped
        }

        public async Task<bool> SendTrackingUpdateNotification(Shipment shipment)
        {
            //Simulate actual sending for tracking update
            await Task.Delay(TimeSpan.FromSeconds(3));

            return true;
        }
    }
}
