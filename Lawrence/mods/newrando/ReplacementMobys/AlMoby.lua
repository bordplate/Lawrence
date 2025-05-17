AlMoby = class("AlMoby", Moby)

function AlMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(909)
    self:SetPosition(295, 240, 34)
    self.rotZ = 1.571

    self.scale = 0.2

    self.disabled = false
end

function AlMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 5 <= player.x and player.x <= self.x + 5 and
            self.y - 5 <= player.y and player.y <= self.y + 5 and
            self.z - 5 <= player.z and player.z <= self.z + 5 then
        return true
    end
    return false
end

function AlMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        if player.totalBolts >= 1000 then
            player:ToastMessage("\x12 Buy \x0cHeli-Pack\x08 for 1,000 bolts", 1)
        else
            player:ToastMessage("You need 1,000 Bolts for the \x0cHeli-Pack\x08", 1)
        end
    end
end

function AlMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) and player.totalBolts >= 1000 then
        player:GiveBolts(-1000)
        player:OnUnlockItem(Item.GetByName("Heli-pack").id, true)
        universe:DistributeSetLevelFlags(2, 3, 78, {[1]=1})
    end
end

function AlMoby:Disable()
    self.disabled = true
end 