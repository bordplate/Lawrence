SteveMoby = class("SteveMoby", Moby)

function SteveMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(851)
    self:SetPosition(268.115, 128.702, 72.609)
    self.rotZ = -1.571

    self.scale = 0.250

    self.disabled = false
end

function SteveMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 5 <= player.x and player.x <= self.x + 5 and
            self.y - 5 <= player.y and player.y <= self.y + 5 and
            self.z - 5 <= player.z and player.z <= self.z + 5 then
        return true
    end
    return false
end

function SteveMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        if player.totalBolts >= 1000 then
            player:ToastMessage("\x12 Buy \x0cPDA\x08 for 1,000 bolts", 1)
        else
            player:ToastMessage("You need 1,000 Bolts for the \x0cPDA\x08", 1)
        end
    end
end

function SteveMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) and player.totalBolts >= 1000 then
        player:GiveBolts(-1000)
        player:OnUnlockItem(Item.GetByName("PDA").id, true)
    end
end

function SteveMoby:Disable()
    self.disabled = true
    self:Delete()
end 