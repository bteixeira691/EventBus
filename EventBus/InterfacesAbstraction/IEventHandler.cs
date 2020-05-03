using EventBus.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.InterfacesAbstraction
{
    public interface IEventHandler<in TEvent> : IEventHandler where TEvent : Event
    {
        Task Handler(TEvent _event);
    }

    public interface IEventHandler
    {

    }

}
