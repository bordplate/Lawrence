using System;
using System.Collections.Generic;
using NLua;

namespace Lawrence
{
    public class Entity
    {
        /// <summary>
        /// Lua counterpart of this object. 
        /// </summary>
        private LuaTable _luaEntity;

        private Dictionary<string, LuaFunction> _luaFunctions = new Dictionary<string, LuaFunction>();

        /// <summary>
        /// Active entities are updated and kept track of by the game.
        /// </summary>
        private bool _active = true;

        public Entity(LuaTable luaEntity = null)
        {
            this._luaEntity = luaEntity;
        }

        public bool IsActive() {
            return _active;
        }

        public void SetActive(bool active) {
            _active = active;
        }

        /// <summary>
        /// Gets a Lua function in the Lua entity reflecting this class and caches it. 
        /// </summary>
        /// <param name="functionName"></param>
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
        private object CallLuaFunction(string functionName, params object[] args) {
            LuaFunction function = GetLuaFunction(functionName);

            if (function == null) {
                return null;
            }

            return function.Call(args);
        }

        public void SetLuaEntity(LuaTable luaEntity) {
            _luaEntity = luaEntity;

            _luaFunctions = new Dictionary<string, LuaFunction>();
        }

        /// <summary>
        /// Calls the Lua object's OnTick function
        /// </summary>
        public void OnTick() {
            if (this._active && _luaEntity == null) {
                return;
            }

            CallLuaFunction("OnTick");
        }
    }
}

