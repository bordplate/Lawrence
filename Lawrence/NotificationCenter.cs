using System;
using System.Collections.Generic;

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

    public class TickNotification : Notification {
        public TickNotification() : base("Tick") { }
    }

    // Class responsible for managing notification subscriptions and posting
    public class NotificationCenter {
        // Dictionary to store subscribers for each notification type
        private Dictionary<string, List<Action<Notification>>> _subscribers;
        private Dictionary<int, Action<Notification>> _indexedSubscribers;

        // Constructor for the NotificationCenter class
        public NotificationCenter() {
            _subscribers = new Dictionary<string, List<Action<Notification>>>();
        }

        // Method to subscribe a callback to a specific notification type
        public void Subscribe<T>(Action<T> callback) where T : Notification {
            string notificationName = typeof(T).Name;
            Action<Notification> wrapperCallback = n => callback((T)n);

            if (!_subscribers.ContainsKey(notificationName)) {
                _subscribers[notificationName] = new List<Action<Notification>>();
            }
            _subscribers[notificationName].Add(wrapperCallback);
        }

        // Method to unsubscribe a callback from a specific notification type
        public void Unsubscribe<T>(Action<T> callback) where T : Notification {
            string notificationName = typeof(T).Name;
            Action<Notification> wrapperCallback = n => callback((T)n);

            if (_subscribers.ContainsKey(notificationName)) {
                _subscribers[notificationName].Remove(wrapperCallback);
            }
        }

        // Method to post a notification to all subscribers of the given type
        public void Post<T>(T notification) where T : Notification {
            string notificationName = typeof(T).Name;
            if (_subscribers.ContainsKey(notificationName)) {
                foreach (var callback in _subscribers[notificationName]) {
                    callback(notification);
                }
            }
        }
    }
}