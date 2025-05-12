using System.Threading.Tasks;

namespace EWP.SF.Item.BusinessLayer
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
        void StartConsumer(string topic, Action<string, string> messageHandler);
    }
}
