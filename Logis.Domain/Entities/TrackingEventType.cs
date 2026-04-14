namespace Logis.Domain.Entities
{
    public enum TrackingEventType
    {
        Created = 0,
        Assigned = 1,
        PickedUp = 2,
        DepartedFacility = 3,
        ArrivedFacility = 4,
        OutForDelivery = 5,
        Delivered = 6,
        Cancelled = 7,
        Note = 8
    }
}