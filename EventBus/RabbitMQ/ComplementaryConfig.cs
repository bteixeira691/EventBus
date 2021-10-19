using System;
using System.Collections.Generic;
using System.Text;

namespace EventBus.RabbitMQ
{
    public class ComplementaryConfig
    {
        public int Retry { get; set; }
        public string QueueName { get; set; }
        public string BrokenName { get; set; }

    }
}
