ViewElement = class("ViewElement", Entity)

function ViewElement:initialize(internalEntity)
    Entity.initialize(self, internalEntity)
    self:SetLuaEntity(self)
end
