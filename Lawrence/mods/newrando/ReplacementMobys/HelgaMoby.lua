HelgaMoby = class("HelgaMoby", Moby)

function HelgaMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(890)
    self:SetPosition(117.218, 83.312, 65.833)
    self.rotZ = 1.787

    self.scale = 0.167

    self.disabled = false
end

function HelgaMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 2 <= player.x and player.x <= self.x + 2 and
            self.y - 2 <= player.y and player.y <= self.y + 2 and
            self.z - 2 <= player.z and player.z <= self.z + 2 then
        return true
    end
    return false
end

function HelgaMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        if player.totalBolts >= 1000 then
            player:ToastMessage("\x12 Buy \x0cSwingshot\x08 for 1,000 bolts ", 1)
        else
            player:ToastMessage("You need 1,000 bolts for the \x0cSwingshot\x08", 1)
        end
    end
end

function HelgaMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) and player.totalBolts >= 1000 then
        player:GiveBolts(-1000)
        player:OnUnlockItem(Item.GetByName("Swingshot").id, true)
    end
end

function HelgaMoby:Disable()
    self.disabled = true
    self:Delete()
end 