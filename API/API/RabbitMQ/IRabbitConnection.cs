namespace API.RabbitMQ
{
    public interface IRabbitConnection
    {
        public bool SendUsingHeaders(string queueName, string exchangeName, string exchangeType, Dictionary<string, object> Headers, byte[]? message);
    }
}