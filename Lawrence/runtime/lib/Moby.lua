Moby = class('Moby', Entity)

Moby.offset = {
    position = {
        x = 0x10, 
        y = 0x14,
        z = 0x18
    },
    rotation = {
        x = 0x40,
        y = 0x44,
        z = 0x48
    },
    state = 0x20,
    modeBits = 0x34,
    animationID = 0x53,
    animationFrameBlendT = 0x54
}

function Moby:initialize(mobyEntity)
    Entity.initialize(self, mobyEntity)
end

function Moby:Universe()
    local universeEntity = self._internalEntity:Universe()
    
    return universeEntity:LuaEntity()
end

function Moby:Level()
    local levelEntity = self._internalEntity:Level()
    
    return levelEntity:LuaEntity()
end
