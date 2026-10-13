using System.Threading.Tasks;
using Confluent.Kafka;

namespace EWP.SF.KafkaSync.BusinessLayer
{
    public interface IKafkaService
    {
        /// <summary>
        /// Produces a message to a Kafka topic
        /// </summary>
        /// <typeparam name="T">Type of the message</typeparam>
        /// <param name="topic">Kafka topic</param>
        /// <param name="key">Message key</param>
        /// <param name="message">Message content</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task ProduceMessageAsync<T>(string topic, string key, T message);

        /// <summary>
        /// Starts a Kafka consumer for the specified topic
        /// </summary>
        /// <param name="topic">Kafka topic to consume</param>
        /// <param name="messageHandler">Action to handle received messages</param>
        //void StartConsumer(string topic, Action<string, string> messageHandler);

        void StartConsumer(string topic, Func<string, string, Task> messageHandler, int? maxRetries = null, int? retryDelayMs = null, string groupId = null);
       
       Task<T> ConsumeMessageAsync<T>(string topic, string groupId, TimeSpan timeout, Func<ConsumeResult<string, string>, bool> predicate = null);

        /// <summary>
        /// Request/response over Kafka without the produce-before-subscribe race of
        /// ProduceMessageAsync + ConsumeMessageAsync: the response topic's end offsets are captured and
        /// assigned to a consumer BEFORE the request is produced, so a fast reply can never be missed
        /// (and a not-yet-existing response topic is created first). Returns default on timeout.
        /// </summary>
        Task<TResponse> RequestAsync<TRequest, TResponse>(string requestTopic, string key, TRequest request, string responseTopic, TimeSpan timeout, Func<ConsumeResult<string, string>, bool> predicate = null);
    }
}
