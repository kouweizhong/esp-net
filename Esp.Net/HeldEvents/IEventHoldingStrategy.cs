﻿#if ESP_EXPERIMENTAL
using Esp.Net.Model;

namespace Esp.Net.HeldEvents
{
    public interface IEventHoldingStrategy<in TModel, in TEvent> where TEvent : IIdentifiableEvent
    {
        bool ShouldHold(TModel model, TEvent @event, IEventContext context);
        IEventDescription GetEventDescription(TModel model, TEvent @event);
    }

    public interface IEventHoldingStrategy<in TModel, in TEvent, in TBaseEvent> : IEventHoldingStrategy<TModel, TEvent>
        where TEvent : IIdentifiableEvent, TBaseEvent
    {
    }
}
#endif