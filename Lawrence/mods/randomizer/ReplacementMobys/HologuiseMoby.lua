HologuiseMoby = class("HologuiseMoby", Moby)

function HologuiseMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(1509)
    self:SetPosition(130, 287, 122)
    self.rotZ = 1.787

    self.scale = 0.2

    self.disabled = false
end

function HologuiseMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 2 <= player.x and player.x <= self.x + 2 and
            self.y - 2 <= player.y and player.y <= self.y + 2 and
            self.z - 2 <= player.z and player.z <= self.z + 2 then
        return true
    end
    return false
end

function HologuiseMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        if self:Universe():LuaEntity().has_hoverboard then
            player:ToastMessage("\x12 Take \x0cHologuise\x08", 1)
        else
            player:ToastMessage("Hoverboard required to earn \x0cHologuise\x08", 1)
        end
    end
end

function HologuiseMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) and self:Universe():LuaEntity().has_hoverboard then
        player:OnUnlockItem(Item.GetByName("Hologuise").id, true)
    end
end

function HologuiseMoby:Disable()
    self.disabled = true
    self:Delete()
end 