InfobotMoby = class("InfobotMoby", Moby)

function InfobotMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(750)

    self.scale = 0.167
    
    self.planet_id = 0x01 -- placeholder value must be changed

    self.disabled = false
end

function InfobotMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 5 <= player.x and player.x <= self.x + 5 and
            self.y - 5 <= player.y and player.y <= self.y + 5 and
            self.z - 5 <= player.z and player.z <= self.z + 5 then
        return true
    end
    return false
end

function InfobotMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        player:ToastMessage("\x12 Take \x0cInfobot\x08", 1)
    end
end

function InfobotMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) then
        player:OnUnlockLevel(self.planet_id)
    end
end

function InfobotMoby:Disable()
    self.disabled = true
    self:Delete()
end 