BobMoby = class("BobMoby",  Moby)

function BobMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(90)
    self:SetPosition(588.527, 579.877, 233.389)
    self.rotZ = 1.787

    self.scale = 0.167

    self.disabled = false
end

function BobMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 5 <= player.x and player.x <= self.x + 5 and
            self.y - 5 <= player.y and player.y <= self.y + 5 and
            self.z - 5 <= player.z and player.z <= self.z + 5 then
        return true
    end
    return false
end

function BobMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        if player.totalBolts >= 2000 then
            player:ToastMessage("\x12 Pay 2,000 Bolts for the \x0cThruster-Pack\x08", 1)
        else
            player:ToastMessage("You need 2,000 Bolts for the \x0cThruster-Pack\x08", 1)
        end
    end
end

function BobMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) and player.totalBolts >= 2000 then
        player:GiveBolts(-2000)
        player:OnUnlockItem(Item.GetByName("Thruster-pack").id, true)
        --self.disabled = true
    end
end

function BobMoby:Disable()
    self.disabled = true
end 