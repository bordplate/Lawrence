CommandoMoby = class("CommandoMoby", Moby)

function CommandoMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(1130)
    self:SetPosition(264.794, 286.884, 36.235)
    self.rotZ = 1.571

    self.scale = 0.208

    self.AnimationId = 2
    
    self.disabled = false
end

function CommandoMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 5 <= player.x and player.x <= self.x + 5 and
            self.y - 5 <= player.y and player.y <= self.y + 5 and
            self.z - 5 <= player.z and player.z <= self.z + 5 then
        return true
    end
    return false
end

function CommandoMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        player:ToastMessage("\x12 Receive \x0cOrxon Infobot\x08", 1)
    end
end

function CommandoMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) then
        player:OnUnlockLevel(0xa)
    end
end

function CommandoMoby:Disable()
    self.disabled = true
    self:Delete()
end 