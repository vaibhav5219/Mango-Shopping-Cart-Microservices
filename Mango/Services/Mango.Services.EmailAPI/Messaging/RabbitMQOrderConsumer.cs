
using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    public class RabbitMQOrderConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private IConnection _connection;
        private IChannel _channel;
        string queueName = string.Empty;
        private string ExchangeName = "";
        private const string OrderCreated_EmailUpdateQueue = "EmailUpdateQueue";

        public RabbitMQOrderConsumer(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
            ExchangeName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {

                var factory = new ConnectionFactory
                {
                    HostName = "localhost",
                    UserName = "guest",
                    Password = "guest"
                };
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                //await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Fanout);
                await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct);
                //queueName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic"); // _channel.CurrentQueue;
                await _channel.QueueDeclareAsync(OrderCreated_EmailUpdateQueue, false, false, false, null);
                await _channel.QueueBindAsync(OrderCreated_EmailUpdateQueue, ExchangeName, "EmailUpdate");

                //await _channel.QueueDeclareAsync(queueName, false, false, false, null);
                //await _channel.QueueBindAsync(queueName, _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic"), string.Empty);

                stoppingToken.ThrowIfCancellationRequested();
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (sender, args) =>
                {
                    var content = Encoding.UTF8.GetString(args.Body.ToArray());
                    RewardsMessage rewardsMessage = JsonConvert.DeserializeObject<RewardsMessage>(content);
                    HandleMessage(rewardsMessage).GetAwaiter().GetResult();

                    _channel.BasicAckAsync(args.DeliveryTag, false);
                };
                await _channel.BasicConsumeAsync(OrderCreated_EmailUpdateQueue, false, consumer);
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.ToString());
            }

            //return Task.CompletedTask;
        }
        private async Task HandleMessage(RewardsMessage rewardsMessage)
        {
            _emailService.LogOrderPlaced(rewardsMessage).GetAwaiter().GetResult();
        }
    }
}
