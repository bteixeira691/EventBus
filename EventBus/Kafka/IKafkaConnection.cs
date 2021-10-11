namespace EventBus.Kafka
{
    public interface IKafkaConnection
    {

        bool IsConnected { get; }

        bool TryConnect();
    }
}