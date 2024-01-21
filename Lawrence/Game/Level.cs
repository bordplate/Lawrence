using System;

namespace Lawrence.Game;

using NLua;

public class Level : Entity {
    private readonly int _gameId;
    private readonly string _name;

    public Level(int gameId, string name, LuaTable luaTable = null) : base(luaTable) {
        _gameId = gameId;
        _name = name;
        
        SetMasksVisibility(true);
    }

    /// <summary>
    /// Friendly name for level.
    /// </summary>
    /// <returns></returns>
    public string Name() {
        return _name;
    }

    /// <summary>
    /// The ID the level has in the game. 
    /// </summary>
    /// <returns></returns>
    public int GameID() {
        return _gameId;
    }

    public LuaTable SpawnMoby(object param) {
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
}