require 'middleclass'

Entity = class('Entity')

--- Initialize by making a new entity in the game and storing the internal C# object. 
function Entity:initialize(internalEntity)
    self._internalEntity = internalEntity

    if internalEntity == nil then
        self._internalEntity = Game:NewEntity(self)
    end
end

--- Functions that don't exist in Lua should redirected to the internal C# object.
function Entity:__index(key)
    local value = Entity[key]

    -- If the key exists in the Entity class, return it
    if value ~= nil then
        return value
    end
    
    local internal = rawget(self, "_internalEntity")[key]
    if internal ~= nil then
        -- If the key does not exist, redirect the call to internal C# object.
        if type(internal) ~= 'userdata' then
            return internal
        end
        
        if (GetTypeName(internal) == "ViewAttribute`1") then
            return internal.Value
        end

        return function(self, ...)
            return rawget(self, "_internalEntity")[key](self._internalEntity, ...)
        end
    end
end

function Entity:__newindex(key, value)
    -- If the key exists in the internal C# object, set it
    -- We ignore OnTick, because of an unknown error that eventually causes a Lua stack overflow
    if rawget(self, '_internalEntity') ~= nil and rawget(self, '_internalEntity')[key] ~= nil then
        if GetTypeName(self._internalEntity[key]) == "ViewAttribute`1" then
            self._internalEntity[key]:Set(value)
            return
        end
        
        self._internalEntity[key] = value
    else
        -- Otherwise, set it in the Entity object
        rawset(self, key, value)
    end
end

--- Makes this entity a different type of entity. 
function Entity:Make(entityType)
    local newEntity = entityType:new(self._internalEntity)

    self = newEntity

    self:SetLuaEntity(newEntity)
    self:ClearLuaCaches()

    self:Made()

    return self
end

function Entity:Is(class)
    return self:isInstanceOf(class)
end

function Entity:FindChildren(type)
    local found = self:FindChildrenInternal(type)
    local children = {}

    if found.Count <= 0 then
        return {}
    end

    for i = 0, found.Count-1, 1 do
        local child = found[i]
        
        children[#children+1] = child
    end
    
    return children
end 