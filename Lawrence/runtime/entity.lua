Entity = class('Entity')

-- Initialize by making a new entity in the game and storing the internal C# object. 
function Entity:initialize()
    self._internalEntity = Game:NewEntity(self)
end

-- Functions that don't exist in Lua should redirect to the internal C# object.
function Entity:__index(key)
    local value = Entity[key]

    -- If the key exists in the Entity class, return it
    if value ~= nil then
        return value
    end

    if self._internalEntity[key] ~= nil then
        -- If the key does not exist, redirect the call to internal C# object.
        if type(self._internalEntity[key]) ~= 'userdata' then
            return self._internalEntity[key]
        end

        return function(self, ...)
            return self._internalEntity[key](self._internalEntity, ...)
        end
    end
end

-- Called every tick for entities that are active and registered in the game. 
function Entity:OnTick()
    print("This is Lua OnTick")
end

local entity = Entity:new()
