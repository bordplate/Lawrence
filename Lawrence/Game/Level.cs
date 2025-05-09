using System;
using System.Collections.Generic;
using Lawrence.Core;

namespace Lawrence.Game;

using NLua;

public class Level : Entity {
    private readonly int _gameId;
    private readonly string _name;
    
    public byte[] LevelFlags1 = new byte[0x10];
    public byte[] LevelFlags2 = new byte[0x100];
    
    private List<Moby> _hybridMobys = new();

    public bool ShouldPropagateLevelFlags = true;

    public Level(int gameId, string name, LuaTable? luaTable = null) : base(luaTable) {
        _gameId = gameId;
        _name = name;
        
        SetMasksVisibility(true);
        
        object levelTable = Game.Shared().State()[name];
        if (levelTable is not LuaTable) {
            levelTable = Game.Shared().State()["Level"];
        }

        if (luaTable == null && levelTable is LuaTable) {
            if (!(((LuaTable)levelTable)["new"] is LuaFunction)) {
                throw new Exception(
                    $"Could not initialize new `Level` entity as initialize isn't a function on `Level` table");
            }

            LuaFunction initializeFunction = ((LuaFunction)((LuaTable)levelTable)["new"]);

            object[] entity = initializeFunction.Call(levelTable, this);

            if (entity.Length <= 0 || !(entity[0] is LuaTable)) {
                throw new Exception("Failed to initialize `Level` Lua entity. `Level` is not a Lua table");
            }

            if (entity[0] is LuaTable levelEntity) {
                SetLuaEntity(levelEntity);
            }
        }
    }

    /// <summary>
    /// Friendly name for level.
    /// </summary>
    /// <returns></returns>
    public string GetName() {
        return _name;
    }

    /// <summary>
    /// The ID the level has in the game. 
    /// </summary>
    /// <returns></returns>
    public int GameID() {
        return _gameId;
    }

    public LuaTable? SpawnMoby(object param) {
        Moby moby = new Moby();
        moby.SetLevel(this);
        
        if (param is int oClass) {
            moby.oClass = oClass;
        }
        
        object mobyTable = Game.Shared().State()["Moby"];
        if (param is LuaTable table) {
            mobyTable = table;
        }

        if (!(mobyTable is LuaTable)) {
            throw new Exception($"Unable to create Moby entity in Lua. Unable to resolve Moby for `{param}`.");
        }

        if (!(((LuaTable)mobyTable)["new"] is LuaFunction)) {
            throw new Exception(
                "Could not initialize new `Moby` entity as initialize isn't a function on `Moby` table");
        }

        LuaFunction initializeFunction = ((LuaFunction)((LuaTable)mobyTable)["new"]);

        object[] entity = initializeFunction.Call(mobyTable, moby);

        if (entity.Length <= 0 || !(entity[0] is LuaTable)) {
            throw new Exception("Failed to initialize `Moby` Lua entity. `Moby` is not a Lua table");
        }

        if (entity[0] is LuaTable mobyEntity) {
            moby.SetLuaEntity(mobyEntity);
            return mobyEntity;
        }

        return null;
    }

    public LuaTable? GetGameMobyByUID(ushort uid) {
        Moby moby = new Moby();
        moby.MakeHybrid(uid);
        moby.SetLevel(this);
        
        object mobyTable = Game.Shared().State()["Moby"];
        if (!(mobyTable is LuaTable)) {
            throw new Exception($"Unable to create Moby entity in Lua.");
        }

        if (!(((LuaTable)mobyTable)["new"] is LuaFunction)) {
            throw new Exception(
                "Could not initialize new `Moby` entity as initialize isn't a function on `Moby` table");
        }

        LuaFunction initializeFunction = (LuaFunction)((LuaTable)mobyTable)["new"];

        object[] entity = initializeFunction.Call(mobyTable, moby);

        if (entity.Length <= 0 || !(entity[0] is LuaTable)) {
            throw new Exception("Failed to initialize `Moby` Lua entity. `Moby` is not a Lua table");
        }

        if (entity[0] is LuaTable mobyEntity) {
            moby.SetLuaEntity(mobyEntity);
            _hybridMobys.Add(moby);
            
            return mobyEntity;
        }
        
        return null;
    }

    /// <summary>
    /// Adds the entity as a child of the level, but without setting this level as its parent.
    /// </summary>
    /// <param name="entity"></param>
    public void Add(Entity entity) {
        Add(entity, false);
    }

    public void Remove(Entity entity, bool unparent = true) {
        base.Remove(entity, unparent);

        if (entity is Moby moby) {
            foreach (Player player in Find<Player>()) {
                player.NotifyDelete(moby);
            }
        }
    }
    
    public List<Moby> GetHybridMobys() {
        return _hybridMobys;
    }

    public void OnFlagChanged(Player originatingPlayer, ushort type, byte size, ushort index, uint value) {
        switch(type) {
            case 1:
                if (index >= LevelFlags1.Length) {
                    Logger.Error($"Level flag index for type 1 out of bounds: {index}");
                    return;
                }
                
                LevelFlags1[index] = (byte)value;
                break;
            case 2:
                if (index >= LevelFlags2.Length) {
                    Logger.Error($"Level flag index for type 2 out of bounds: {index}");
                    return;
                }
                LevelFlags2[index] = (byte)value;
                break;
            default:
                Logger.Error($"Unknown level flag type: {type}");
                break;
        }

        if (ShouldPropagateLevelFlags) {
            foreach (Player player in Find<Player>()) {
                if (player == originatingPlayer) {
                    continue;
                }

                if (player.Level() != this) {
                    Logger.Error("Player level mismatch");
                    return;
                }
                
                
                player.ChangedLevelFlag((byte)type, index, value);
            }
        }
    }
}