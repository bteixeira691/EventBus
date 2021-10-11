using EventBus.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.InterfacesAbstraction
{
    public interface IEventBus
    {
        Task Publish(Event _event);

        Task Subscribe<T, TH>() where T : Event where TH : IEventHandler<T>;
    }
}
