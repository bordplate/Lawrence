using System;
using System.Collections.Generic;
using NLua;

namespace Lawrence
{
    public class GameMode
    {
        private LuaTable _gameMode;

        private Dictionary<string, LuaFunction> _luaFunctions = new Dictionary<string, LuaFunction>();

        public GameMode(LuaTable gameMode)
        {
            _gameMode = gameMode;
        }

        /// <summary>
        /// Gets a Lua function in the Lua entity reflecting this class and caches it. 
        /// </summary>
        /// <param name="functionName"></param>
        private LuaFunction GetLuaFunction(string functionName) {
            if (_gameMode == null) {
                return null;
            }

            if (_luaFunctions.ContainsKey(functionName)) {
                return _luaFunctions[functionName];
            }

            if (!(_gameMode["class"] is LuaTable)) {
                return null;
            }

            LuaTable classTable = (LuaTable)_gameMode["class"];

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

        public object Call(string functionName, params object[] args) {
            LuaFunction function = GetLuaFunction(functionName);

            if (function == null) {
                return null;
            }

            return function.Call(args);
        }

        public int GetInt(string key) {
            if (!(_gameMode[key] is float)) {
                throw new Exception($"No float for key {key}");
            }

            return (int)_gameMode[key];
        }

        public bool GetBool(string key) {
            if (!(_gameMode[key] is bool)) {
                throw new Exception($"No bool for key {key}");
            }

            return (bool)_gameMode[key];
        }
    }
}

