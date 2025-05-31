ScientistMoby = class("ScientistMoby", Moby)

function ScientistMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(1105)
    self:SetPosition(170.824, 323.326, 142.704)
    self.rotZ = -0.919

    self.scale = 0.125

    self.disabled = false
end

function ScientistMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 5 <= player.x and player.x <= self.x + 5 and
            self.y - 5 <= player.y and player.y <= self.y + 5 and
            self.z - 5 <= player.z and player.z <= self.z + 5 then
        return true
    end
    return false
end

function ScientistMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        if player.totalBolts >= 1000 then
            player:ToastMessage("\x12 Buy \x0cGrindboots\x08 for 2,000 bolts", 1)
        else
            player:ToastMessage("You need 2,000 Bolts for the \x0cGrindboots\x08", 1)
        end
    end
end

function ScientistMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) and player.totalBolts >= 2000 then
        player:GiveBolts(-2000)
        player:OnUnlockItem(Item.GetByName("Grindboots").id, true)
    end
end

function ScientistMoby:Disable()
    self.disabled = true
    self:Delete()
end 