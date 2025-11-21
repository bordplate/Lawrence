using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using Lawrence.Game;

namespace Lawrence.Core;
    
// Abstract base class for notifications
public abstract class Notification {
    // Property to store the notification name
    public string Name { get; }

    // Constructor for the Notification class
    protected Notification(string name) {
        Name = name;
    }
}

// Class representing a player connected notification, inherits from Notification
public class PlayerJoinedNotification : Notification {
    // Property to store the player entity
    public Entity Entity { get; }

    // Constructor for the PlayerConnectedNotification class
    public PlayerJoinedNotification(int id, string? username, Entity entity)
        : base("PlayerJoined") {
        Entity = entity;
    }
}

public class PlayerDisconnectedNotification : Notification {
    // Property to store the player entity
    public Entity Entity { get; }

    // Constructor for the PlayerConnectedNotification class
    public PlayerDisconnectedNotification(int id, string username, Entity entity)
        : base("PlayerDisconnected") {
        Entity = entity;
    }
}

public class PrimaryUniverseChangedNotification : Notification {
    public readonly Universe Universe;

    public PrimaryUniverseChangedNotification(Universe universe) : base("PrimaryUniverseChanged") {
        Universe = universe;
    }
}

public class DeleteEntityNotification : Notification {
    public readonly Entity Entity;

    public DeleteEntityNotification(Entity entity) : base("DeleteEntity") {
        Entity = entity;
    }
}

public class PreTickNotification : Notification {
    public PreTickNotification() : base("PreTick") { }
}

public class TickNotification : Notification {
    public TickNotification() : base("Tick") { }
}

public class PostTickNotification : Notification {
    public PostTickNotification() : base("PostTick") { }
}

/// <summary>
/// Class responsible for managing notification subscriptions and posting
/// </summary>
public class NotificationCenter {
    // Dictionary to store subscribers for each notification type
    private readonly ConcurrentDictionary<string, List<Delegate>> _subscribers;

    private readonly Mutex _lock = new Mutex();

    /// <summary>
    /// Constructor for the NotificationCenter class
    /// </summary>
    public NotificationCenter() {
        _subscribers = new ConcurrentDictionary<string, List<Delegate>>();
    }

    /// <summary>
    /// Method to subscribe a callback to a specific notification type
    /// </summary>
    /// <param name="callback"></param>
    /// <typeparam name="T"></typeparam>
    public void Subscribe<T>(Action<T> callback) where T : Notification {
        string notificationName = typeof(T).Name;

        _lock.WaitOne();
        if (_subscribers.TryGetValue(notificationName, out var callbacks)) {
            callbacks.Add(callback);
        } else {
            _subscribers[notificationName] = new List<Delegate> { callback };
        }
        _lock.ReleaseMutex();
    }

    /// <summary>
    /// Method to unsubscribe a callback from a specific notification type
    /// </summary>
    /// <param name="callback"></param>
    /// <typeparam name="T"></typeparam>
    public void Unsubscribe<T>(Action<T> callback) where T : Notification {
        string notificationName = typeof(T).Name;
        if (_subscribers.TryGetValue(notificationName, out var callbacks)) {
            callbacks.Remove(callback);
        }
    }

    public void UnsubscribeAll(Entity entity) {
        var callbacksToRemove = new List<(string, Delegate)>();
        
        foreach (var subscriber in _subscribers) {
            foreach (var callback in subscriber.Value) {
                if (callback.Target == entity) {
                    callbacksToRemove.Add((subscriber.Key, callback));
                }
            }
        }

        foreach (var callback in callbacksToRemove) {
            if (_subscribers.TryGetValue(callback.Item1, out var callbacks)) {
                callbacks.Remove(callback.Item2);
            }
        }
    }

    /// <summary>
    /// Method to post a notification to all subscribers of the given type
    /// </summary>
    /// <param name="notification"></param>
    /// <typeparam name="T"></typeparam>
    public void Post<T>(T notification) where T : Notification {
        string notificationName = typeof(T).Name;
        if (_subscribers.TryGetValue(notificationName, out var callbacks)) {
            // We use GetRange on callbacks here because otherwise we'd crash due to "modified collection" during
            //   the loop. 
            foreach (Action<T> callback in callbacks.GetRange(0, callbacks.Count)) {
                callback(notification);
            }
        }
    }
    
    /// <summary>
    /// Post notifications of type T only to objects of type E
    /// </summary>
    /// <param name="notification"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    public void Post<T, E>(T notification) where T : Notification {
        string notificationName = typeof(T).Name;
        if (_subscribers.TryGetValue(notificationName, out var callbacks)) {
            // We use GetRange on callbacks here because otherwise we'd crash due to "modified collection" during
            //   the loop. 
            foreach (Action<T> callback in callbacks.GetRange(0, callbacks.Count)) {
                if (callback.Target is E) {
                    callback(notification);
                }
            }
        }
    }
}
