using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzDOEpicPauseChildren.Classes
{
    public class WebhookBody
    {
        public string subscriptionId { get; set; }
        public int notificationId { get; set; }
        public string id { get; set; }
        public string eventType { get; set; }
        public string publisherId { get; set; }
        public Message message { get; set; }
        public Detailedmessage detailedMessage { get; set; }
        public Resource resource { get; set; }
        public string resourceVersion { get; set; }
        public Resourcecontainers resourceContainers { get; set; }
        public DateTime createdDate { get; set; }
    }

}
