﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace MicroserviceB.Event
{
    public class EventFromMicroserviceA : EventBus.Events.Event
    {
        public string Name { get; set; }

        public string Message { get; set; }
    }
}
