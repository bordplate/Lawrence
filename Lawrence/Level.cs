namespace Lawrence;

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

    /// <summary>
    /// Adds the entity as a child of the level, but without setting this level as its parent.
    /// </summary>
    /// <param name="entity"></param>
    public void Add(Entity entity) {
        this.Add(entity, false);
    }
}