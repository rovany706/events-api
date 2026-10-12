namespace EventManager.Contracts;

public static class KafkaTopics
{
    public const string BookingConfirmedTopicName = "booking-confirmed";
    public const string BookingCancelledTopicName = "booking-cancelled";
    public const string BookingRejectedTopicName = "booking-rejected";
}