NanotechVendorMoby = class("NanotechVendorMoby", Moby)

function NanotechVendorMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(1326)
    self:SetPosition(155.7, 190.7, 54)
    self.rotZ = 1.571

    self.scale = 0.2

    self.disabled = false
    
    self.selling = "premium"
end

function NanotechVendorMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 5 <= player.x and player.x <= self.x + 5 and
            self.y - 5 <= player.y and player.y <= self.y + 5 and
            self.z - 5 <= player.z and player.z <= self.z + 5 then
        return true
    end
    return false
end

function NanotechVendorMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        if self.selling == "premium" then
            if player.totalBolts >= 4000 then
                player:ToastMessage("\x12 Buy \x0cPremium Nanotech\x08 for 4,000 bolts", 1)
            else
                player:ToastMessage("You need 4,000 Bolts for \x0cPremium Nanotech\x08", 1)
            end
        elseif self.selling == "ultra" then
            if player.totalBolts >= 30000 then
                player:ToastMessage("\x12 Buy \x0cUltra Nanotech\x08 for 30,000 bolts", 1)
            else
                player:ToastMessage("You need 30,000 Bolts for \x0cUltra Nanotech\x08", 1)
            end
        else
            print("error, nanotech vendor disabling self")
            self:Disable() -- something went wrong
        end
    end
end

function NanotechVendorMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) then
        if self.selling == "premium" and player.totalBolts >= 4000 then
            player:GiveBolts(-4000)
            player:OnUnlockItem(Item.GetByName("Premium Nanotech").id, true)
        elseif self.selling == "ultra" and player.totalBolts >= 30000 then
            player:GiveBolts(-30000)
            player:OnUnlockItem(Item.GetByName("Ultra Nanotech").id, true)
        else
            print("error, nanotech vendor disabling self")
            self:Disable() -- something went wrong
        end
    end
end

function NanotechVendorMoby:Progress()
    self.selling = "ultra"
end

function NanotechVendorMoby:Disable()
    self.disabled = true
end 