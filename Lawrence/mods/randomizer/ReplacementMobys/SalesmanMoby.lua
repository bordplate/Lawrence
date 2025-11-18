SalesmanMoby = class("SalesmanMoby", Moby)

function SalesmanMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(925)
    self:SetPosition(415.014, 295.218, 61.844)
    self.rotZ = 1.621

    self.scale = 0.167

    self.disabled = false
end

function SalesmanMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 5 <= player.x and player.x <= self.x + 5 and
            self.y - 5 <= player.y and player.y <= self.y + 5 and
            self.z - 5 <= player.z and player.z <= self.z + 5 then
        return true
    end
    return false
end

function SalesmanMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        if player.lobby.universe.totalBolts >= 1000 then
            player:ToastMessage("\x12 Buy \x0cR.Y.N.O\x08 for 150,000 bolts", 1)
        else
            player:ToastMessage("You need 150,000 Bolts for the \x0cR.Y.N.O\x08", 1)
        end
    end
end

function SalesmanMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) and universe.totalBolts >= 150000 then
        universe:GiveBolts(-150000)
        player:OnUnlockItem(Item.GetByName("R.Y.N.O.").id, true)
    end
end

function SalesmanMoby:Disable()
    self.disabled = true
    self:Delete()
end 