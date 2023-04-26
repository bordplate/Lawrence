using System;
using System.Collections.Generic;
using NLua;

namespace Lawrence
{
    /// <summary>
    /// 
    /// </summary>
    public class Universe : Entity
    {
        /// <summary>
        /// Primary universes are universes that handle notifications about when players join. There should only be 1 primary universe at the time.
        /// </summary>
        bool _primary = false;

        public Universe(LuaTable luaTable) : base(luaTable) {
            Game.Shared().NotificationCenter().Subscribe<PrimaryUniverseChangedNotification>(OnPrimaryUniverseChanged);
            Game.Shared().NotificationCenter().Subscribe<PlayerJoinedNotification>(OnPlayerJoined);
        }

        public void Start(bool primary) {
            // If we set this as primary universe, we notify the other ones to tell them they should not be primary anymore. 
            if (primary) {
                _primary = primary;
                Game.Shared().NotificationCenter().Post<PrimaryUniverseChangedNotification>(new PrimaryUniverseChangedNotification(this));
            }
        }

        public void OnPrimaryUniverseChanged(PrimaryUniverseChangedNotification notification) {
            if (notification.Universe != this) {
                _primary = false;
            }
        }

        public void OnPlayerJoined(PlayerJoinedNotification notification) {
            // Notification ignored if we're not primary universe. 
            if (!_primary) {
                return;
            }

            this.Add(notification.Entity);

            CallLuaFunction("OnPlayerJoin", new object[] { notification.Entity });
        }

        public override void OnTick(TickNotification notification)
        {
            base.OnTick(notification);
        }
    }
}

