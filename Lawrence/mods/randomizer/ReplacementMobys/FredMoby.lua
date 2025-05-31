FredMoby = class("FredMoby", Moby)

function FredMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(298)
    self:SetPosition(547.5, 387, 153.99)
    self.rotZ = 1.787

    self.scale = 0.750

    self.disabled = false
    self.was_close_before = false
end

function FredMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 2 <= player.x and player.x <= self.x + 2 and
            self.y - 2 <= player.y and player.y <= self.y + 2 and
            self.z - 2 <= player.z and player.z <= self.z + 2 then
        return true
    end
    return false
end

function FredMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        if not self.was_close_before then
            player.lobby.universe:DistributeSetLevelFlags(1, 11, 4, {0xff})
            self.was_close_before = true
        end
        if self:Universe():LuaEntity().has_raritanium then
            player:ToastMessage("\x12 Trade \x0cRaritanium\x08 for \x0cPersuader\x08", 1)
        else
            player:ToastMessage("You need \x0cRaritanium\x08", 1)
        end
    end
end

function FredMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) and self:Universe():LuaEntity().has_raritanium then
        player:OnUnlockItem(Item.GetByName("Persuader").id, true)
    end
end

function FredMoby:Disable()
    print("disabling fred")
    self.disabled = true
    self:Delete()
end 