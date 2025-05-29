BouncerMoby = class("BouncerMoby", Moby)

function BouncerMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(919)
    self:SetPosition(250.266, 201.119, 99.750)
    self.rotZ = 1.787

    self.scale = 0.167

    self.disabled = false
end

function BouncerMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 2 <= player.x and player.x <= self.x + 2 and
            self.y - 2 <= player.y and player.y <= self.y + 2 and
            self.z - 2 <= player.z and player.z <= self.z + 2 then
        return true
    end
    return false
end

function BouncerMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        if player.totalBolts >= 1000 then
            player:ToastMessage("\x12 Bribe Bouncer with 4,000 bolts ", 1)
        else
            player:ToastMessage("You need 4,000 bolts to see Captain Qwark", 1)
        end
    end
end

function BouncerMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) and player.totalBolts >= 4000 then
        player:GiveBolts(-4000)
        player:OnUnlockLevel(0x07)
    end
end

function BouncerMoby:Disable()
    self.disabled = true
    self:Delete()
end 