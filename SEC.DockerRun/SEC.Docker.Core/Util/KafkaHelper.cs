using Confluent.Kafka;

namespace SEC.Docker.Core
{
    public class KafkaHelper
    {
        IProducer<string, string>? producer;
        private IProducer<string, string> _Producer
        {
            get
            {
                return producer ??= new ProducerBuilder<string, string>(
                    new ProducerConfig
                    {
                        BootstrapServers = "10.164.18.217:9092,10.164.18.216:9092,10.164.18.218:9092"
                    }
                    )
                   .SetErrorHandler((_, e) =>
                   {

                   })
                   .Build();
            }
        }
        public void ProduceMessage(string topic, string message)
        {
            _Producer.Produce(topic, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = message
            }, r =>
            {
                if (r.Error.IsError)
                {

                } 
            });
        }
        IConsumer<Ignore, string>? consumer;
        private IConsumer<Ignore, string> _Consumer
        {
            get
            {
                return consumer ??= new ConsumerBuilder<Ignore, string>(new ConsumerConfig
                {
                    GroupId = "DockerSupporter",
                    BootstrapServers = "",
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoOffsetStore = false
                }).SetErrorHandler((_, e) =>
                {

                }).Build();
            }
        }

        CancellationTokenSource cts = new CancellationTokenSource();
        public string ConsumeResultMessage()
        {
            var consumeResult = _Consumer.Consume(cts.Token);
            _Consumer.StoreOffset(consumeResult);
            return consumeResult.Message.Value;
        }



    }
}
