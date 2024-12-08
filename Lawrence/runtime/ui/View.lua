View = class("View", Entity)

function View:initialize(player)
    if not player:Is(Player) then
        error("View:initialize(player) requires a Player instance as argument 1")
    end
    
    Entity.initialize(self, NativeView(player))
    self:SetLuaEntity(self)
end

function View:OnLoad()
    
end

function View:AddElement(element)
    self._internalEntity:AddElement(element._internalEntity)
end
