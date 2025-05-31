PilotHelmetMoby = class("PilotHelmetMoby", Moby)

function PilotHelmetMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(1290)
    self:SetPosition(345.138, 313.903, 42.016)
    self.rotZ = 1.787

    self.scale = 0.146

    self.rotationSpeed = 0.01*math.pi

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

function PilotHelmetMoby:OnTick()
    self.rotZ = self.rotZ + self.rotationSpeed * Game:DeltaTime()  -- update rotation
end