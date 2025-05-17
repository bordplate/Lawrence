ZoomeratorMoby = class("ZoomeratorMoby", Moby)

function ZoomeratorMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(1002)
    self:SetPosition(303, 222, 101)
    self.rotZ = 1.787

    self.scale = 0.2

    self.disabled = false
end

function ZoomeratorMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 2 <= player.x and player.x <= self.x + 2 and
            self.y - 2 <= player.y and player.y <= self.y + 2 and
            self.z - 2 <= player.z and player.z <= self.z + 2 then
        return true
    end
    return false
end

function ZoomeratorMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        if player.has_hoverboard then
            player:ToastMessage("\x12 Take \x0cZoomerator\x08", 1)
        else
            player:ToastMessage("Hoverboard required to earn \x0cZoomerator\x08", 1)
        end
    end
end

function ZoomeratorMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) and player.has_hoverboard then
        player:OnUnlockItem(Item.GetByName("Zoomerator").id, true)
    end
end

function ZoomeratorMoby:Disable()
    self.disabled = true
    self:Delete()
end 