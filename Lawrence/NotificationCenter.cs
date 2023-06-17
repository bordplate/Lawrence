using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

namespace Lawrence {
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
        public PlayerJoinedNotification(int id, string username, Entity entity)
            : base("PlayerJoined") {
            Entity = entity;
        }
    }

    public class PrimaryUniverseChangedNotification : Notification {
        public Universe Universe;

        public PrimaryUniverseChangedNotification(Universe universe) : base("PrimaryUniverseChanged") {
            Universe = universe;
        }
    }

    public class DeleteEntityNotification : Notification {
        public Entity Entity;

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
        private ConcurrentDictionary<string, List<Delegate>> _subscribers;

        private Mutex _lock = new Mutex();

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
    }
}