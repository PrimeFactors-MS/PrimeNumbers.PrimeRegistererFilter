using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PrimeNumbers.PrimeRegistererFilter.Core;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace PrimeNumbers.PrimeRegistererFilter.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IConnection _connection;
        private IModel _channel;
        private PrimeSubmitter _primeSubmitter;
        private PrimeFilter _primeFilter;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string primesDbUrl = "http://primes-db-service:30006";
            ConnectionFactory factory = new()
            {
                UserName = "guest",
                Password = "guest",
                HostName = "rabbitmq-msprimes-service",
                Port = 30101,
                ConsumerDispatchConcurrency = 5,
                DispatchConsumersAsync = true,
                VirtualHost = "primeRegistration",
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            AsyncEventingBasicConsumer consumer = new (_channel);
            consumer.Received += OnMessageReceived;

            _primeSubmitter = PrimeSubmitter.CreateNew(new Uri(primesDbUrl));
            _primeFilter = new PrimeFilter(_primeSubmitter);

            _channel.BasicConsume("PrimeRegistererFilter.UnfilteredInput", true, consumer);

            return Task.CompletedTask;
        }

        private async Task OnMessageReceived(object sender, BasicDeliverEventArgs @event)
        {
            PrimeRecordReport recordReport;
            try
            {
                recordReport = JsonSerializer.Deserialize<PrimeRecordReport>(@event.Body.Span);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Serialization of message failded");
                return;
            }

            try
            {
                await _primeFilter.FilterPrimeRecord(recordReport);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed filtering prime record");
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            DoIgnoreException(_channel.Close);
            DoIgnoreException(_connection.Close);
            DoIgnoreException(_primeSubmitter.Dispose);
            return base.StopAsync(cancellationToken);
        }

        private static void DoIgnoreException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception) { }
        }
    }

    

}
