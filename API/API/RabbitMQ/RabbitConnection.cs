using RabbitMQ.Client;
using System.Reflection;
using System.Text;

namespace API.RabbitMQ
{
    public class RabbitConnection : IRabbitConnection
    {
        // https://stackoverflow.com/questions/15033848/how-can-a-rabbitmq-client-tell-when-it-loses-connection-to-the-server
        private string host = "localhost";
        private int port = 5672;
        private ConnectionFactory _factory;
        private IConnection connection;

        private bool isConnected = false;
        private static IModel channel = null;
        private Dictionary<string, IModel> channelDict = new Dictionary<string, IModel>();
        private readonly ILogger<RabbitConnection> _logger;

        public RabbitConnection(ILogger<RabbitConnection> logger)
        {
            _logger = logger;
            _factory = new ConnectionFactory { HostName = host, Port = port, RequestedHeartbeat = TimeSpan.FromSeconds(16) };

        }
        private IConnection CreateConnection()
        {
            IConnection createdConnection = null;
            while (!isConnected)
            {
                try
                {
                    // https://www.rabbitmq.com/heartbeats.html <-- docs says values between 5 and 20 seconds are optimal
                    createdConnection = _factory.CreateConnection();
                    isConnected = true;
                    _logger.LogInformation("createConnection(): Connected to RabbitMQ at " + host + ":" + port);
                }
                catch (Exception e)
                {
                    _logger.LogInformation("createConnection(): Connection attempt to RabbitMQ at " + host + ":" + port + " failed");
                    _logger.LogInformation(e.Message);
                }
                Thread.Sleep(1500);
            }
            return createdConnection;
        }
        public IModel CreateChannel(string exchange, string exchangeType)
        {
            //TODO: make this use the input parameter exchangeType
            // "direct", "fanout", "headers", "topic"
            if (isConnected)
            {
                _logger.LogInformation("RabbitConnection.createChannel(string exchange, string exchangeType): is connected");
                channel = connection.CreateModel();
                channel.ExchangeDeclare(exchange: exchange, type: exchangeType, true, autoDelete: false);
                return channel;
            }
            else
            {
                _logger.LogInformation("createChannel(): no connection to RabbitMQ");
                throw new Exception("no connection to RabbitMQ");
            }
        }

        // exchange, type

        private void OnConnectionLost(object? sender, ShutdownEventArgs reason)
        {
            isConnected = false;
            connection = CreateConnection();
        }

        public IModel GetChannel(string exchange, string exchangeType)
        {
            string channelKey = CreateChannelKey(exchange, exchangeType);


            if (connection is null)
            {
                connection = CreateConnection();
                connection.ConnectionShutdown += OnConnectionLost;
            }

            if (channelDict.ContainsKey(channelKey))
            {
                _logger.LogInformation("getChannel(): channel exists");
                return channelDict[channelKey];
            }

            else
            {
                IModel createdChannel = CreateChannel(exchange, exchangeType);
                channelDict.Add(channelKey, createdChannel);
                _logger.LogInformation("getChannel(): channel created");
                return channelDict[channelKey];
            }
        }

        public bool SendExchange(string exchange, string exchangeType, string routingKey, string message)
        {
            bool result = false;
            string channelKey = CreateChannelKey(exchange, exchangeType);
            IModel channel = GetChannel(exchange, exchangeType);
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: exchange,
                                    routingKey: routingKey,
                                    basicProperties: null,
                                    body: body);

            return result;
        }

        public bool SendUsingHeaders(string queueName, string exchangeName, string exchangeType, Dictionary<string, object> headers, byte[]? message)
        {
            string channelKey = CreateChannelKey(exchangeName, exchangeType);
            IModel channel = GetChannel(exchangeName, exchangeType);
            //Properties
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "", headers);

            channel.BasicPublish(exchange: exchangeName,
                                 routingKey: "",
                                 basicProperties: properties,
                                 body: message);
            return true;

        }

        private string CreateChannelKey(string exchange, string exchangeType)
        {
            return exchange + "+" + exchangeType;
        }
    }
}
