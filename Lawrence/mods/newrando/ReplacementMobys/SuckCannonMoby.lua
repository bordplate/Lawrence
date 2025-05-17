SuckCannonMoby = class("SuckCannonMoby", Moby)

function SuckCannonMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(1120)
    self:SetPosition(324, 245, 63)
    self.rotZ = 1.787

    self.scale = 0.2

    self.disabled = false
end

function SuckCannonMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 2 <= player.x and player.x <= self.x + 2 and
            self.y - 2 <= player.y and player.y <= self.y + 2 and
            self.z - 2 <= player.z and player.z <= self.z + 2 then
        return true
    end
    return false
end

function SuckCannonMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        player:ToastMessage("\x12 Get \x0cSuck cannon\x08", 1)
    end
end

function SuckCannonMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) then
        player:OnUnlockItem(Item.GetByName("Suck Cannon").id, true)
    end
end

function SuckCannonMoby:Disable()
    self.disabled = true
    self:Delete()
end 