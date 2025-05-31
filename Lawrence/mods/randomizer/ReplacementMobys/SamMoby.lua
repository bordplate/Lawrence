SamMoby = class("SamMoby", Moby)

function SamMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(924)
    self:SetPosition(203.171, 191.666, 50.5)
    self.rotZ = 1.787

    self.scale = 0.250

    self.disabled = false
end

function SamMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 4 <= player.x and player.x <= self.x + 4 and
            self.y - 4 <= player.y and player.y <= self.y + 4 and
            self.z - 4 <= player.z and player.z <= self.z + 4 then
        return true
    end
    return false
end

function SamMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        if player.totalBolts >= 2000 then
            player:ToastMessage("\x12 Buy \x0cInfobot\x08 for 2,000 bolts ", 1)
        else
            player:ToastMessage("You need 2,000 bolts to buy the \x0cInfobot\x08", 1)
        end
    end
end

function SamMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) and player.totalBolts >= 2000 then
        player:GiveBolts(-2000)
        player:OnUnlockLevel(0x0f)
    end
end

function SamMoby:Disable()
    self.disabled = true
    self:Delete()
end 