DeserterMoby = class("DeserterMoby", Moby)

function DeserterMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(1144)
    self:SetPosition(305.839, 95.922, 37.416)
    self.rotZ = -0.115

    self.scale = 0.250

    self.AnimationId = 1
    
    self.disabled = false
end

function DeserterMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 2 <= player.x and player.x <= self.x + 2 and
            self.y - 2 <= player.y and player.y <= self.y + 2 and
            self.z - 2 <= player.z and player.z <= self.z + 2 then
        return true
    end
    return false
end

function DeserterMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        if player.lobby.universe.totalBolts >= 1000 then
            player:ToastMessage("\x12 Buy \x0cInfobot\x08 for 2,000 bolts ", 1)
        else
            player:ToastMessage("You need 2,000 bolts for the \x0cInfobot\x08", 1)
        end
    end
end

function DeserterMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) and universe.totalBolts >= 2000 then
        universe:GiveBolts(-2000)
        player:OnUnlockLevel(0x09)
    end
end

function DeserterMoby:Disable()
    self.disabled = true
    self:Delete()
end 