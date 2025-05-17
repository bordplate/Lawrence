SkidMoby = class("SkidMoby", Moby)

function SkidMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(788)
    self:SetPosition(182, 130, 26)
    self.rotZ = 1.571

    self.scale = 0.2

    self.disabled = false
end

function SkidMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 5 <= player.x and player.x <= self.x + 5 and
            self.y - 5 <= player.y and player.y <= self.y + 5 and
            self.z - 5 <= player.z and player.z <= self.z + 5 then
        return true
    end
    return false
end

function SkidMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        player:ToastMessage("\x12 Skid's script is fucked, get \x0cHoverboard\x08", 1)
    end
end

function SkidMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) then
        player:OnUnlockItem(Item.GetByName("Hoverboard").id, false)
    end
end

function SkidMoby:Disable()
    self.disabled = true
    self:Delete()
end 