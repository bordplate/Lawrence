MagnebootsMoby = class("MagnebootsMoby", Moby)

function MagnebootsMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(18)
    self:SetPosition(231, 166, 57)
    self.rotZ = 1.787

    self.scale = 0.2

    self.disabled = false
end

function MagnebootsMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 2 <= player.x and player.x <= self.x + 2 and
            self.y - 2 <= player.y and player.y <= self.y + 2 and
            self.z - 2 <= player.z and player.z <= self.z + 2 then
        return true
    end
    return false
end

function MagnebootsMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        player:ToastMessage("\x12 Take \x0cMagneboots\x08", 1)
    end
end

function MagnebootsMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) then
        player:OnUnlockItem(Item.GetByName("Magneboots").id, true)
    end
end

function MagnebootsMoby:Disable()
    self.disabled = true
    self:Delete()
end 