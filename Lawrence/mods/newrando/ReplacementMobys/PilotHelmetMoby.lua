PilotHelmetMoby = class("PilotHelmetMoby", Moby)

function PilotHelmetMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(1290)
    self:SetPosition(345, 313, 41)
    self.rotZ = 1.787

    self.scale = 0.2

    self.disabled = false
end

function PilotHelmetMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 2 <= player.x and player.x <= self.x + 2 and
            self.y - 2 <= player.y and player.y <= self.y + 2 and
            self.z - 2 <= player.z and player.z <= self.z + 2 then
        return true
    end
    return false
end

function PilotHelmetMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        player:ToastMessage("\x12 Take \x0cPilot's Helmet\x08", 1)
    end
end

function PilotHelmetMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) then
        player:OnUnlockItem(Item.GetByName("Pilot's Helmet").id, true)
    end
end

function PilotHelmetMoby:Disable()
    self.disabled = true
    self:Delete()
end 