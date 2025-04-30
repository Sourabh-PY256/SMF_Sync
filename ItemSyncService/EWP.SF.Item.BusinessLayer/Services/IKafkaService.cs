using System.Threading.Tasks;

namespace EWP.SF.Item.BusinessLayer.Services
{
    public interface IKafkaService
    {
        Task ProduceMessageAsync<T>(string topic, string key, T message);
        void StartConsumer(string topic, Action<string, string> messageHandler);
    }
}