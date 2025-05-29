View = class("View", Entity)

function View:initialize()
    Entity.initialize(self, NativeView())
    self:SetLuaEntity(self)
end

function View:OnLoad()
    
end

function View:AddElement(element)
    self._internalEntity:AddElement(element._internalEntity)
end
