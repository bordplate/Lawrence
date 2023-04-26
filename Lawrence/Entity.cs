using System;
using System.Collections.Generic;
using System.Linq;
using NLua;

namespace Lawrence
{
    public partial class Entity {
        private Guid _guid = Guid.NewGuid();

        /// <summary>
        /// Lua counterpart of this object. 
        /// </summary>
        LuaTable _luaEntity;

        Entity _parent;
        List<Entity> _children = new List<Entity>();

        private Dictionary<string, LuaFunction> _luaFunctions = new Dictionary<string, LuaFunction>();

        /// <summary>
        /// Active entities are updated and kept track of by the game.
        /// </summary>
        private bool _active = true;

        public Entity(LuaTable luaEntity = null) {
            this._luaEntity = luaEntity;

            Game.Shared().NotificationCenter().Subscribe<TickNotification>(OnTick);
        }

        #region Getters/setters

        public Guid GUID() {
            return _guid;
        }

        public bool IsActive() {
            return _active;
        }

        public void SetActive(bool active) {
            _active = active;
        }

        #endregion
    }

    #region Lua
    partial class Entity {

        /// <summary>
        /// Gets a Lua function in the Lua entity reflecting this class and caches it. 
        /// </summary>
        /// <param name="functionName">Name of the function to get.</param>
        private LuaFunction GetLuaFunction(string functionName) {
            if (_luaEntity == null) {
                return null;
            }

            if (_luaFunctions.ContainsKey(functionName)) {
                return _luaFunctions[functionName];
            }

            if (!(_luaEntity["class"] is LuaTable)) {
                return null;
            }

            LuaTable classTable = (LuaTable)_luaEntity["class"];

            if (!(classTable["__declaredMethods"] is LuaTable)) {
                return null;
            }

            LuaTable declaredMethods = (LuaTable)classTable["__declaredMethods"];

            if (declaredMethods[functionName] != null && declaredMethods[functionName] is LuaFunction) {
                LuaFunction function = (LuaFunction)declaredMethods[functionName];

                _luaFunctions.Add(functionName, function);

                return function;
            }

            return null;
        }

        /// <summary>
        /// Tries to get Lua function from the Lua entity and calls it using the provided args. 
        /// </summary>
        /// <param name="functionName">Name of the function to call</param>
        /// <param name="args">Args to pass to the function</param>
        /// <returns>null if not found or whatever the called function returns (often null).</returns>
        protected object CallLuaFunction(string functionName, params object[] args) {
            LuaFunction function = GetLuaFunction(functionName);

            if (function == null) {
                return null;
            }

            return function.Call(args);
        }

        /// <summary>
        /// Sets the internal Lua entity and clears cached values that were specific to the old Lua entity.
        /// </summary>
        /// <param name="luaEntity"></param>
        public void SetLuaEntity(LuaTable luaEntity) {
            _luaEntity = luaEntity;

            _luaFunctions = new Dictionary<string, LuaFunction>();
        }

        /// <summary>
        /// Calls the Lua object's OnTick function
        /// </summary>
        public virtual void OnTick(TickNotification notification) {
            if (!this._active || _luaEntity == null) {
                return;
            }

            CallLuaFunction("OnTick");
        }
    }
    #endregion

    #region Hierarchy
    partial class Entity { 
        public Entity Parent() {
            return _parent;
        }

        public void Add(Entity entity) {
            entity._parent = this;

            _children.Add(entity);
        }

        public void Add(IEnumerable<Entity> entities) {
            foreach (Entity entity in entities) {
                Add(entity);
            }
        }

        public IEnumerable<T> Find<T>() where T : Entity {
            List<T> entities = new List<T>();

            foreach (var entity in _children) {
                if (entity is T) {
                    yield return (T)entity;
                }

                // Recursively search the children
                foreach (var e in entity.Find<T>()) {
                    yield return (T)e;
                }
            }
        }

        public IEnumerable<Entity> Where(Func<Entity, bool> predicate) {
            return _children.Where(predicate);
        }

     }
    #endregion

    #region Networking
    partial class Entity {
        public virtual void SendPacket((MPPacketHeader, byte[]) packet) {
            if (this is Player) {
                throw new InvalidOperationException(
                    "Base Entity:SendPacket() method called on a Player entity. " +
                    "Likely this is due to the Player:SendPacket override calling its base function. " +
                    "This could have caused an infinite loop."
                );
            }

            foreach (Player player in Find<Player>()) {
                player.SendPacket(packet);
            }
        }
    }
    #endregion
}