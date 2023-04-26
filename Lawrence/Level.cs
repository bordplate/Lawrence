namespace Lawrence;

using NLua;

public class Level : Entity {
    private readonly int _gameId;
    private readonly string _name;

    public Level(int gameId, string name, LuaTable luaTable = null) : base(luaTable) {
        _gameId = gameId;
        _name = name;
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
    public int GetGameID() {
        return _gameId;
    }
}