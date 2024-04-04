using Mango.Services.EmailAPI.Messaging;
using System.Runtime.CompilerServices;

namespace Mango.Services.EmailAPI.Extension
{
    public static class ApplicationBuilderExtensions
    {
        private static IAzureServiceBusConsumer ServiceBusConsumer { get; set; }
        public static IApplicationBuilder UseAzureServiceBusConsumer(this IApplicationBuilder app) 
        {
            ServiceBusConsumer = app.ApplicationServices.GetService<IAzureServiceBusConsumer>();
            var hostApplicationLife = app.ApplicationServices.GetService<IHostApplicationLifetime>();

            hostApplicationLife.ApplicationStarted.Register(onStart);
            hostApplicationLife.ApplicationStopped.Register(onStop);

            return app;
        }

        private static void onStop()
        {
            ServiceBusConsumer.Stop();
        }

        private static void onStart()
        {
            ServiceBusConsumer?.Start();
        }
    }
}
