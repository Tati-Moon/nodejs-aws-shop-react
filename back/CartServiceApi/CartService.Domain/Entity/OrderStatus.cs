namespace CartService.Domain.Entity
{
    public enum OrderStatus
    {
        Open,
        InProgress,
        Approved,
        Confirmed,
        Sent,
        Completed,
        Cancelled
    }
}
