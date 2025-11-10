EdwinaMoby = class("EdwinaMoby", Moby)

function EdwinaMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(328)
    self:SetPosition(416.297, 521.419, 36.294)
    self.rotZ = -1.596

    self.scale = 0.250

    self.disabled = false
end

function EdwinaMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 5 <= player.x and player.x <= self.x + 5 and
            self.y - 5 <= player.y and player.y <= self.y + 5 and
            self.z - 5 <= player.z and player.z <= self.z + 5 then
        return true
    end
    return false
end

function EdwinaMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        if player.lobby.universe.totalBolts >= 2000 then
            player:ToastMessage("\x12 Pay 2,000 for the \x0cHydro-Pack\x08", 1)
        else
            player:ToastMessage("You need 2,000 Bolts for the \x0cHydro-Pack\x08", 1)
        end
    end
end

function EdwinaMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) and universe.totalBolts >= 2000 then
        universe:GiveBolts(-2000)
        player:OnUnlockItem(Item.GetByName("Hydro-pack").id, true)
    end
end

function EdwinaMoby:Disable()
    self.disabled = true
end 