using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace Mango.Services.OrderAPI.RabbitMQSender
{
    public class RabbitMQOrderMessageSender : IRabbitMQOrderMessageSender
    {
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private IConnection _connection;
        private const string OrderCreated_RewardsUpdateQueue = "RewardsUpdateQueue";
        private const string OrderCreated_EmailUpdateQueue = "EmailUpdateQueue";
        public RabbitMQOrderMessageSender()
        {
            _hostName = "localhost";
            _userName = "guest";
            _password = "guest";
        }
        public async void SendMessage(object message, string exchangeName)
        {
            try
            {
                if (await ConnectionExists())
                {
                    using var channel = await _connection.CreateChannelAsync();
                    // await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, durable:false);
                    await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, durable:false);

                    await channel.QueueDeclareAsync(OrderCreated_EmailUpdateQueue, false, false, false, null);
                    await channel.QueueDeclareAsync(OrderCreated_RewardsUpdateQueue, false, false, false, null);

                    await channel.QueueBindAsync(OrderCreated_EmailUpdateQueue, exchangeName, "EmailUpdate");
                    await channel.QueueBindAsync(OrderCreated_RewardsUpdateQueue, exchangeName, "RewardsUpdate");

                    var json = JsonConvert.SerializeObject(message);
                    var body = Encoding.UTF8.GetBytes(json);

                    //await channel.BasicPublishAsync(exchange: exchangeName, routingKey: string.Empty, body: body);
                    await channel.BasicPublishAsync(exchange: exchangeName, routingKey: "EmailUpdate", body: body);
                    await channel.BasicPublishAsync(exchange: exchangeName, routingKey: "RewardsUpdate", body: body);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private async Task CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostName,
                    UserName = _userName,
                    Password = _password
                };

                _connection = await factory.CreateConnectionAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private async Task<bool> ConnectionExists()
        {
            if(_connection != null)
            {
                return true;
            }
            CreateConnection().Wait();
            return true;
        }
    }
}
