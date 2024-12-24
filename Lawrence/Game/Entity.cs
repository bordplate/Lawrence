using System;
using System.Collections.Generic;
using System.Linq;
using NLua;

using Lawrence.Core;
using Lawrence.Game.UI;
using NLua.Exceptions;

namespace Lawrence.Game;

partial class Entity {
    private readonly List<Label> _labels = new();
    
    public virtual void AddLabel(Label label) {
        _labels.Add(label);
        
        foreach (Player player in Find<Player>()) {
            player.AddLabel(label);
        }
    }

    public virtual void RemoveLabel(Label label) {
        _labels.Remove(label);
        
        foreach (Player player in Find<Player>()) {
            player.RemoveLabel(label);
        }
    }

    public void AddLabel(LuaTable tableLabel) {
        object internalEntity = tableLabel["_internalEntity"];

        if (internalEntity is Label label) {
            AddLabel(label);
        }
        else {
            Logger.Error("Failed to add a Label. Label was not correct type.");
        }
    }

    public void RemoveLabel(LuaTable tableLabel) {
        object internalEntity = tableLabel["_internalEntity"];

        if (internalEntity is Label label) {
            RemoveLabel(label);
        }
        else {
            Logger.Error("Failed to remove a Label. Label was not correct type.");
        }
    }
}

partial class Entity {
    public virtual void SendPacket(Packet packet) {
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

    public void BlockGoldBolt(int planet, int number) {
        SendPacket(Packet.MakeBlockGoldBoltPacket(planet, number));
    }
}

partial class Entity { 
    public Entity? Parent() {
        return _parent;
    }

    public bool HasParent(Entity entity) {
        Entity? next = this;
        do {
            if (next == entity) {
                return true;
            }
            
            next = next.Parent();
        } while (next != null);

        return false;
    }

    public void AddEntity(LuaTable entityTable) {
        var internalEntity = entityTable["_internalEntity"];

        if (internalEntity is Entity entity) {
            Add(entity);
        }
    }

    public virtual void Add(Entity entity, bool reparent = true) {
        if (reparent) {
            entity._parent?.Remove(entity);

            entity._parent = this;
        }

        if (entity is Player player) {
            foreach (Label label in _labels) {
                player.AddLabel(label);
            }
        }

        _children.Add(entity);
    }

    public void Add(IEnumerable<Entity> entities) {
        foreach (Entity entity in entities) {
            Add(entity);
        }
    }

    public virtual void Remove(Entity entity, bool unparent = true) {
        if (unparent) {
            entity._parent = null;
        }
        
        _children.Remove(entity);
    }

    public IEnumerable<T> Find<T>() where T : Entity {
        List<Entity> removeEntities = new List<Entity>();

        foreach (var entity in _children) {
            // Can't remove deleted while enumerating, remove later
            if (entity._deleted) {
                removeEntities.Add(entity);
                continue;
            }
            
            if (entity is T) {
                yield return (T)entity;
            }

            // Recursively search the children
            foreach (var e in entity.Find<T>()) {
                yield return (T)e;
            }
        }
        
        // Remove deleted entities
        if (removeEntities.Count > 0)
            _children.RemoveAll(x => removeEntities.Contains(x));
    }

    public List<LuaTable> FindChildrenInternal(string entityType) {
        Type? type = Type.GetType($"Lawrence.Game.{entityType}, Lawrence");
        List<Entity> removeEntities = new List<Entity>();
        List<LuaTable> entities = new List<LuaTable>();

        foreach (var entity in _children) {
            // Can't remove deleted while enumerating, remove later
            if (entity._deleted) {
                removeEntities.Add(entity);
                continue;
            }

            if (entity.LuaEntity() == null) {
                continue;
            }
            
            if (entity.GetType() == type) {
                if (entity.LuaEntity() != null) entities.Add(entity.LuaEntity()!);
            }

            // Recursively search the children
            foreach (var e in entity.FindChildrenInternal(entityType)) {
                if (entity.LuaEntity() != null) entities.Add(entity.LuaEntity()!);
            }
        }
        
        // Remove deleted entities
        if (removeEntities.Count > 0)
            _children.RemoveAll(x => removeEntities.Contains(x));

        return entities;
    }

    public IEnumerable<Entity> Where(Func<Entity, bool> predicate) {
        return _children.Where(predicate);
    }

    public bool IsInstanced() {
        return _instanced;
    }

    public void SetInstanced(bool instanced) {
        _instanced = instanced;
    }
}

partial class Entity {
    private LuaTable? _luaClass;
    private Dictionary<string, LuaFunction>? _declaredMethods;
    private List<LuaTable> _superClasses = new();
    private Dictionary<LuaTable, Dictionary<string, LuaFunction>> _superDeclaredMethods = new();

    private LuaFunction? _onTickFunction;
    
    public void ClearLuaCaches() {
        _luaClass = null;
        _declaredMethods = null;
        _onTickFunction = null;
        _superClasses.Clear();
        _superDeclaredMethods.Clear();
        _luaFunctions.Clear();
    }

    /// <summary>
    /// Gets a Lua function in the Lua entity reflecting this class and caches it. 
    /// </summary>
    /// <param name="functionName">Name of the function to get.</param>
    private LuaFunction? GetLuaFunction(string functionName) {
        if (_luaEntity == null) {
            return null;
        }

        if (_luaFunctions.TryGetValue(functionName, out var luaFunction)) {
            return luaFunction;
        }

        if (GetDeclaredMethods() is not {} declaredMethods) {
            return null;
        }

        if (GetDeclaredMethod(functionName) is { } func) {
            _luaFunctions.Add(functionName, func);

            return func;
        }

        foreach (var superTable in GetSuperClasses()) {
            if (GetSuperDeclaredMethod(superTable, functionName) is { } function) {
                _luaFunctions.Add(functionName, function);

                return function;
            }
        }

        return null;
    }
    
    public LuaTable? GetLuaClass() {
        if (_luaClass == null && _luaEntity != null) {
            _luaClass = (LuaTable)_luaEntity["class"];
        }

        return _luaClass;
    }
    
    public Dictionary<string, LuaFunction>? GetDeclaredMethods() {
        if (_declaredMethods == null && GetLuaClass() is {} luaClass && luaClass["__declaredMethods"] is LuaTable declaredMethods) {
            _declaredMethods = new Dictionary<string, LuaFunction>();

            foreach (var key in declaredMethods.Keys) {
                if (declaredMethods[key] is LuaFunction func && key is string key_) {
                    _declaredMethods.Add(key_, func);
                }
            }
        }

        return _declaredMethods;
    }
    
    public LuaFunction? GetDeclaredMethod(string methodName) {
        if (GetDeclaredMethods() is {} declaredMethods) {
            if (declaredMethods.TryGetValue(methodName, out var func)) {
                return func;
            }
        }

        return null;
    }
    
    public List<LuaTable> GetSuperClasses() {
        if (_superClasses.Count == 0 && _luaEntity != null) {
            if (GetLuaClass() is {} classTable) {
                while (classTable["super"] is LuaTable superTable) {
                    _superClasses.Add(superTable);

                    classTable = superTable;
                }
            }
        }

        return _superClasses;
    }
    
    public Dictionary<string, LuaFunction>? GetSuperDeclaredMethods(LuaTable superTable) {
        if (_superDeclaredMethods.TryGetValue(superTable, out var declared)) {
            return declared;
        }
        
        if (superTable["__declaredMethods"] is LuaTable superDeclaredMethods) {
            var methods = new Dictionary<string, LuaFunction>();

            foreach (var key in superDeclaredMethods.Keys) {
                if (superDeclaredMethods[key] is LuaFunction func && key is string key_) {
                    methods.Add(key_, func);
                }
            }

            _superDeclaredMethods.Add(superTable, methods);

            return methods;
        }
        
        return null;
    }
    
    public LuaFunction? GetSuperDeclaredMethod(LuaTable superTable, string methodName) {
        if (GetSuperDeclaredMethods(superTable) is {} declaredMethods) {
            if (declaredMethods.TryGetValue(methodName, out var func)) {
                return func;
            }
        }

        return null;
    }

    /// <summary>
    /// Tries to get Lua function from the Lua entity and calls it using the provided args. 
    /// </summary>
    /// <param name="functionName">Name of the function to call</param>
    /// <param name="args">Args to pass to the function</param>
    /// <returns>null if not found or whatever the called function returns (often null).</returns>
    protected void CallLuaFunction(string functionName, params object?[] args) {
        if (GetLuaFunction(functionName) is { } function) {
            function.Call(args);
            Game.Shared().State().State.Pop(1);
        }
    }
    
    protected object? CallLuaFunctionWithResult(string functionName, params object?[] args) {
        var classNamespace = ((LuaTable)_luaEntity!["class"])["name"] as string;
        
        Logger.Log($"{classNamespace}:{functionName}() PreGet: {Game.Shared().State().State.GetTop()}");
        
        var function = GetLuaFunction(functionName);
        
        Logger.Log($"{classNamespace}:{functionName}() PreCall: {Game.Shared().State().State.GetTop()}");
        
        var result = function?.Call(args);
        
        Logger.Log($"{classNamespace}:{functionName}() PostCall: {Game.Shared().State().State.GetTop()}");
        
        return result;
    }

    /// <summary>
    /// Sets the internal Lua entity and clears cached values that were specific to the old Lua entity.
    /// </summary>
    /// <param name="luaEntity"></param>
    public void SetLuaEntity(LuaTable luaEntity) {
        _luaEntity = luaEntity;

        _luaFunctions = new Dictionary<string, LuaFunction>();
    }

    public LuaTable? LuaEntity() {
        return _luaEntity;
    }

    /// <summary>
    /// Calls the Lua object's OnTick function.
    /// If this entity is a visibility group, it tells children that are players what's up with all the other
    ///     children that are mobys. 
    /// </summary>
    public virtual void OnTick(TickNotification notification) {
        if (!_active || _luaEntity == null) {
            return;
        }
        
        try {
            if (_onTickFunction == null) {
                _onTickFunction = GetLuaFunction("OnTick");
            }

            if (_onTickFunction is { } tickFunction) {
                tickFunction.Call(_luaEntity);
                Game.Shared().State().State.Pop(1);
            }
        } catch (LuaScriptException e) {
            Logger.Error($"Error in OnTick for entity: {e.Message}");
        }
    }
}

/// <summary>
/// An Entity is an object that is tracked across native (C#) and Lua environment. It is tightly coupled with a Lua
///     table `_luaEntity`. This is to simplify coding in the Lua environment, but we can run a lot of native code
///     to improve performance. 
/// </summary>
public partial class Entity {
    private Guid _guid = Guid.NewGuid();

    /// <summary>
    /// Lua counterpart of this object. 
    /// </summary>
    LuaTable? _luaEntity;

    private Entity? _parent;
    List<Entity> _children = new();

    private bool _maskVisibility;
    private bool _maskCollision = false;

    /// <summary>
    /// Instanced Entities are only visible to parents
    /// </summary>
    private bool _instanced;

    private readonly long _startTicks = Game.Shared().Ticks();

    private Dictionary<string, LuaFunction> _luaFunctions = new();

    /// <summary>
    /// Active entities are updated and kept track of by the game.
    /// </summary>
    private bool _active = true;
    
    private bool _deleted;

    public Entity(LuaTable? luaEntity = null) {
        _luaEntity = luaEntity;

        Game.Shared().NotificationCenter().Subscribe<TickNotification>(OnTick);
        Game.Shared().NotificationCenter().Subscribe<DeleteEntityNotification>(OnDeleteEntity);
    }

    /// <summary>
    /// Marks this Entity for deletion so that it's from parents next time they iterate their children.
    /// </summary>
    public virtual void Delete() {
        if (_luaEntity != null) {
            _luaEntity.Dispose();
            _luaEntity = null;
        }

        _active = false;
        _deleted = true;
        
        Game.Shared().NotificationCenter().Unsubscribe<TickNotification>(OnTick);
        Game.Shared().NotificationCenter().Unsubscribe<DeleteEntityNotification>(OnDeleteEntity);

        Game.Shared().NotificationCenter().Post(new DeleteEntityNotification(this));
    }

    public virtual void OnDeleteEntity(DeleteEntityNotification notification) {
        if (notification.Entity == this)
            return;
        Remove(notification.Entity);
    }

    public virtual void DeleteAllChildrenWithOClass(ushort oClass) {
        SendPacket(Packet.MakeDeleteAllMobysPacket(oClass));
    }
    
    public virtual void DeleteAllChildrenWithUID(ushort uid) {
        SendPacket(Packet.MakeDeleteAllMobysUIDPacket(uid));
    }

    #region Getters/setters

    public Guid GUID() {
        return _guid;
    }

    public bool IsActive() {
        return _active;
    }

    public bool IsDeleted() {
        return _deleted;
    }

    public void SetActive(bool active) {
        _active = active;
    }

    public bool MasksVisibility() {
        return _maskVisibility;
    }

    public void SetMasksVisibility(bool maskVisibility) {
        _maskVisibility = maskVisibility;
    }

    public long Ticks() {
        return Game.Shared().Ticks() - _startTicks;
    }

    #endregion
}
