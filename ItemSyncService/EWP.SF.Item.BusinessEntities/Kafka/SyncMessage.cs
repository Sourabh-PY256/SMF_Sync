using EWP.SF.Common.Models;

namespace EWP.SF.Item.BusinessEntities.Kafka
{
    /// <summary>
    /// Message class for Kafka sync operations
    /// </summary>
    public class SyncMessage
    {
        /// <summary>
        /// The service identifier to execute
        /// </summary>
        public string Service { get; set; }
        
        /// <summary>
        /// The trigger type (SmartFactory, Erp, etc.)
        /// </summary>
        public string Trigger { get; set; }
        
        /// <summary>
        /// The execution type (1 = Event, 0 = SyncButton)
        /// </summary>
        public int ExecutionType { get; set; }
        
        /// <summary>
        /// The user who initiated the sync
        /// </summary>
        public User User { get; set; }
        
        /// <summary>
        /// The entity code for the sync operation
        /// </summary>
        public string EntityCode { get; set; }
        
        /// <summary>
        /// The body data for the sync operation
        /// </summary>
        public string BodyData { get; set; }
    }
}