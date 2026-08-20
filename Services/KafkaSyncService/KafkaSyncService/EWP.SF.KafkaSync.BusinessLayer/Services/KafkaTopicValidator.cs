using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.BusinessLayer
{
    public class KafkaTopicValidator
    {
        private readonly ILogger<KafkaTopicValidator> _logger;
        private readonly string _bootstrapServers;

        public KafkaTopicValidator(ILogger<KafkaTopicValidator> logger, string bootstrapServers)
        {
            _logger = logger;
            _bootstrapServers = bootstrapServers;
        }

        public async Task EnsureTopicsExistAsync(IEnumerable<string> topics, int numPartitions = 1, short replicationFactor = 1)
        {
            using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = _bootstrapServers }).Build();

            try
            {
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
                var existingTopics = new HashSet<string>();
                foreach (var topicMetadata in metadata.Topics)
                {
                    existingTopics.Add(topicMetadata.Topic);
                }

                var topicsToCreate = new List<TopicSpecification>();

                foreach (var topic in topics)
                {
                    if (!existingTopics.Contains(topic))
                    {
                        _logger.LogWarning("Topic {Topic} does not exist. Creating...", topic);
                        topicsToCreate.Add(new TopicSpecification
                        {
                            Name = topic,
                            NumPartitions = numPartitions,
                            ReplicationFactor = replicationFactor
                        });
                    }
                    else
                    {
                        _logger.LogInformation("Topic {Topic} already exists.", topic);
                    }
                }

                if (topicsToCreate.Count > 0)
                {
                    await adminClient.CreateTopicsAsync(topicsToCreate);
                    _logger.LogInformation("Created {Count} new topics.", topicsToCreate.Count);
                }
            }
            catch (CreateTopicsException ex)
            {
                foreach (var result in ex.Results)
                {
                    _logger.LogError("An error occurred creating topic {Topic}: {Reason}", result.Topic, result.Error.Reason);
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ensure topics exist.");
                throw;
            }
        }
    }
}
