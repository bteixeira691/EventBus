using EventBus.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBus.InterfacesAbstraction
{
    public interface IEventBus
    {
        void Publish(Event _event);

        void Subscribe<T, TH>() where T : Event where TH : IEventHandler<T>;

        void Unsubscribe<T, TH>() where T : Event where TH : IEventHandler<T>;
    }
}
