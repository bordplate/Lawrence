Moby = class('Moby', Entity)

function Moby:initialize(mobyEntity)
    Entity.initialize(self, mobyEntity)
end

function Moby:Universe()
    print("Finding universe")
    local universeEntity = self._internalEntity:Universe()
    
    return universeEntity:LuaTable()
end

function Moby:OnTick()
    Entity.OnTick(self)
end 