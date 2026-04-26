//===========================================================================
// File:        EventAggregator.cs
// Project:     ClassLibrary
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Lightweight in-process event aggregator implementing the
//              publish-subscribe pattern with string-keyed event names.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

namespace ClassLibrary;

/// <summary>
/// Implements a simple publish-subscribe event aggregator that allows
/// loosely coupled components to communicate via named events.
/// </summary>
public class EventAggregator
{
    private readonly Dictionary<string, List<Action<object>>> _handlers = new();
    private const string EventPrefix = "event://";

    /// <summary>
    /// Registers a handler to be invoked when an event with the given
    /// name is published.
    /// </summary>
    /// <param name="eventName">The name of the event to subscribe to.</param>
    /// <param name="handler">The callback to invoke on publication.</param>
    public void Subscribe(string eventName, Action<object> handler)
    {
        var key = EventPrefix + eventName;
        if (!_handlers.ContainsKey(key))
            _handlers[key] = new List<Action<object>>();
        _handlers[key].Add(handler);
    }

    /// <summary>
    /// Publishes an event, invoking every handler currently subscribed to
    /// the given event name.
    /// </summary>
    /// <param name="eventName">The name of the event being published.</param>
    /// <param name="data">The payload passed to each subscribed handler.</param>
    public void Publish(string eventName, object data)
    {
        var key = EventPrefix + eventName;
        if (_handlers.TryGetValue(key, out var handlers))
            foreach (var handler in handlers)
                handler(data);
    }

    /// <summary>
    /// Removes all handlers registered for the given event name.
    /// </summary>
    /// <param name="eventName">The name of the event to clear.</param>
    public void Unsubscribe(string eventName)
    {
        var key = EventPrefix + eventName;
        _handlers.Remove(key);
    }

    /// <summary>
    /// Returns the number of handlers currently registered for the given
    /// event name.
    /// </summary>
    /// <param name="eventName">The name of the event to query.</param>
    /// <returns>The number of registered handlers, or <c>0</c> if none.</returns>
    public int HandlerCount(string eventName)
    {
        var key = EventPrefix + eventName;
        return _handlers.TryGetValue(key, out var h) ? h.Count : 0;
    }
}