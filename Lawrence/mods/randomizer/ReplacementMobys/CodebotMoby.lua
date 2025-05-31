CodebotMoby = class("CodebotMoby", Moby)

function CodebotMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(1428)
    self:SetPosition(499.594, 360.236, 140.5)
    self.rotZ = 1.605

    self.scale = 0.167

    self.disabled = false
end

function CodebotMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 2 <= player.x and player.x <= self.x + 2 and
            self.y - 2 <= player.y and player.y <= self.y + 2 and
            self.z - 2 <= player.z and player.z <= self.z + 2 then
        return true
    end
    return false
end

function CodebotMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        player:ToastMessage("\x12 Get \x0cCodebot\x08", 1)
    end
end

function CodebotMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) then
        player:OnUnlockItem(Item.GetByName("Codebot").id, true)
    end
end

function CodebotMoby:Disable()
    self.disabled = true
    self:Delete()
end 