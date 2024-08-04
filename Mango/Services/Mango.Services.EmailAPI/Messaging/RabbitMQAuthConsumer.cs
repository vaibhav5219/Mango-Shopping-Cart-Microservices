
using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    public class RabbitMQAuthConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private IConnection _connection;
        private IChannel _channel;

        public RabbitMQAuthConsumer(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
            
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

                string queueName = _configuration.GetValue<string>("TopicAndQueueNames:registerUserQueue");

                await _channel.QueueDeclareAsync("registerUser", false, false, false, null);

                stoppingToken.ThrowIfCancellationRequested();
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (sender, args) =>
                {
                    var content = Encoding.UTF8.GetString(args.Body.ToArray());
                    string email = JsonConvert.DeserializeObject<string>(content);
                    HandleMessage(email).GetAwaiter().GetResult();

                    _channel.BasicAckAsync(args.DeliveryTag, false);
                };
                await _channel.BasicConsumeAsync(_configuration.GetValue<string>("TopicAndQueueNames:registerUserQueue"), false, consumer);
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.ToString());
            }

            //return Task.CompletedTask;
        }
        private async Task HandleMessage(string email)
        {
            _emailService.RegisterUserEmailAndLog(email).GetAwaiter().GetResult();
        }
    }
}
