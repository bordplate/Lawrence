MinerMoby = class("MinerMoby", Moby)

function MinerMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(282)
    self:SetPosition(263.8, 279, 52.8)
    self.rotZ = 1.787

    self.scale = 0.2

    self.disabled = false
end

function MinerMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 2 <= player.x and player.x <= self.x + 2 and
            self.y - 2 <= player.y and player.y <= self.y + 2 and
            self.z - 2 <= player.z and player.z <= self.z + 2 then
        return true
    end
    return false
end

function MinerMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        player:ToastMessage("\x12 Receive \x0cRaritanium\x08", 1)
    end
end

function MinerMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) then
        player:OnUnlockItem(Item.GetByName("Raritanium").id, true)
    end
end

function MinerMoby:Disable()
    self.disabled = true
    self:Delete()
end 