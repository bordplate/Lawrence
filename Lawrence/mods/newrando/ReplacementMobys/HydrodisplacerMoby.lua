HydrodisplacerMoby = class("HydrodisplacerMoby", Moby)

function HydrodisplacerMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(1016)
    self:SetPosition(212, 195, 131.5)
    self.rotZ = 1.787

    self.scale = 0.2

    self.disabled = false
end

function HydrodisplacerMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 2 <= player.x and player.x <= self.x + 2 and
            self.y - 2 <= player.y and player.y <= self.y + 2 and
            self.z - 2 <= player.z and player.z <= self.z + 2 then
        return true
    end
    return false
end

function HydrodisplacerMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        player:ToastMessage("\x12 Take \x0cHydrodisplacer\x08", 1)
    end
end

function HydrodisplacerMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) then
        player:OnUnlockItem(Item.GetByName("Hydrodisplacer").id, true)
        universe:DistributeSetLevelFlags(1, 6, 1, {0xff}) -- gadgebot gate
        -- bridge flag???
    end
end

function HydrodisplacerMoby:Disable()
    self.disabled = true
    self:Delete()
end 