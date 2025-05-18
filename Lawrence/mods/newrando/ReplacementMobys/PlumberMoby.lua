PlumberMoby = class("PlumberMoby", Moby)

function PlumberMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(774)
    self:SetPosition(251, 187, 96)
    self.rotZ = 1.787

    self.scale = 0.2

    self.disabled = false
end

function PlumberMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 2 <= player.x and player.x <= self.x + 2 and
            self.y - 2 <= player.y and player.y <= self.y + 2 and
            self.z - 2 <= player.z and player.z <= self.z + 2 then
        return true
    end
    return false
end

function PlumberMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        if player.totalBolts >= 500 then
            player:ToastMessage("\x12 Buy \x0cInfobot\x08 for 500 bolts ", 1)
        else
            player:ToastMessage("You need 500 bolts to buy the \x0cInfobot\x08", 1)
        end
    end
end

function PlumberMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) and player.totalBolts >= 500 then
        player:GiveBolts(-500)
        player:OnUnlockLevel(0x02)
    end
end

function PlumberMoby:Disable()
    self.disabled = true
    self:Delete()
end 