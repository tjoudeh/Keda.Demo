namespace Keda.Demo.Contracts
{
    public class Shipment
    {
        public Guid ShipmentId { get; set; }
        public string? WaybillNo { get; set; }
        public DateTime CreatedOn { get; set; }
        public Recipient Recipient { get; set; } = new Recipient();
        public TrackingUpdate TrackingUpdate { get; set; } = new TrackingUpdate();

    }
}