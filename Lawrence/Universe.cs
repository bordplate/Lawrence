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
        private bool _primary = false;

        private List<Level> _levels = new List<Level> {
            new Level(0, "Veldin1"),
            new Level(1, "Novalis"),
            new Level(2, "Aridia"),
            new Level(3, "Kerwan"),
            new Level(4, "Eudora"),
            new Level(5, "Rilgar"),
            new Level(6, "BlargStation"),
            new Level(7, "Umbris"),
            new Level(8, "Batalia"),
            new Level(9, "Gaspar"),
            new Level(10, "Orxon"),
            new Level(11, "Pokitaru"),
            new Level(12, "Hoven"),
            new Level(13, "GemlikStation"),
            new Level(14, "Oltanis"),
            new Level(15, "Quartu"),
            new Level(16, "KaleboIII"),
            new Level(17, "DreksFleet"),
            new Level(18, "Veldin2")
        };

        public Universe(LuaTable luaTable) : base(luaTable) {
            Game.Shared().NotificationCenter().Subscribe<PrimaryUniverseChangedNotification>(OnPrimaryUniverseChanged);
            Game.Shared().NotificationCenter().Subscribe<PlayerJoinedNotification>(OnPlayerJoined);
        }

        public override void Delete() {
            Game.Shared().NotificationCenter().Unsubscribe<PrimaryUniverseChangedNotification>(OnPrimaryUniverseChanged);
            Game.Shared().NotificationCenter().Unsubscribe<PlayerJoinedNotification>(OnPlayerJoined);
            
            base.Delete();
        }

        public override void Add(Entity entity, bool reparent = true) {
            base.Add(entity, reparent);

            if (entity is Player || entity.GetType().IsSubclassOf(typeof(Player))) {
                CallLuaFunction("OnPlayerJoin", new object[] { LuaEntity(), entity.LuaEntity() });
            }
        }

        public Level GetLevelByName(string levelName) {
            foreach (Level level in _levels) {
                if (levelName == level.GetName()) {
                    return level;
                }
            }

            return null;
        }

        public Level GetLevelByGameID(ushort levelID) {
            foreach (Level level in _levels) {
                if (levelID == level.GetGameID()) {
                    return level;
                }
            }

            return null;
        }

        public void Start(bool primary) {
            // If we set this as primary universe, we notify the other ones to tell them they should not be primary anymore. 
            if (primary) {
                _primary = true;
                Game.Shared().NotificationCenter().Post<PrimaryUniverseChangedNotification>(new PrimaryUniverseChangedNotification(this));
            }
        }

        public void SetPrimary(bool primary) {
            _primary = primary;
            
            if (primary) {
                Game.Shared().NotificationCenter().Post<PrimaryUniverseChangedNotification>(new PrimaryUniverseChangedNotification(this));
            }
        }

        private void OnPrimaryUniverseChanged(PrimaryUniverseChangedNotification notification) {
            if (notification.Universe != this) {
                _primary = false;
            }
        }

        private void OnPlayerJoined(PlayerJoinedNotification notification) {
            // Notification ignored if we're not primary universe. 
            if (!_primary) {
                return;
            }

            this.Add(notification.Entity);
        }

        public override void OnTick(TickNotification notification)
        {
            base.OnTick(notification);
        }
    }
}

