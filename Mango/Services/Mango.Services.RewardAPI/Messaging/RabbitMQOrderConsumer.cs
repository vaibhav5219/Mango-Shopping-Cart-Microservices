using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.RewardAPI.Messaging
{
    public class RabbitMQOrderConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly RewardService _rewardService;
        private IConnection _connection;
        private IChannel _channel;
        private const string OrderCreated_RewardsUpdateQueue = "RewardsUpdateQueue";
        private string ExchangeName = "";

        public RabbitMQOrderConsumer(IConfiguration configuration, RewardService rewardService)
        {
            _configuration = configuration;
            _rewardService = rewardService;
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
                await _channel.QueueDeclareAsync(OrderCreated_RewardsUpdateQueue, false, false, false, null);
                await _channel.QueueBindAsync(OrderCreated_RewardsUpdateQueue, ExchangeName, "RewardsUpdate");

                stoppingToken.ThrowIfCancellationRequested();
                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += (sender, args) =>
                {
                    var content = Encoding.UTF8.GetString(args.Body.ToArray());
                    RewardsMessage rewardsMessage = JsonConvert.DeserializeObject<RewardsMessage>(content);
                    HandleMessage(rewardsMessage).GetAwaiter().GetResult();

                    _channel.BasicAckAsync(args.DeliveryTag, false);
                    return Task.CompletedTask;
                };
                await _channel.BasicConsumeAsync(OrderCreated_RewardsUpdateQueue, false, consumer);
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.ToString());
            }

            //return Task.CompletedTask;
        }
        private async Task HandleMessage(RewardsMessage rewardsMessage)
        {
            _rewardService.UpdateRewards(rewardsMessage).GetAwaiter().GetResult();
        }
    }
}
