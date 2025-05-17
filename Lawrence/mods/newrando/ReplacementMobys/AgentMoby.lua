AgentMoby = class("AgentMoby", Moby)

function AgentMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(786)
    self:SetPosition(258, 266, 26)
    self.rotZ = 1.787

    self.scale = 0.2

    self.disabled = false
end

function AgentMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 2 <= player.x and player.x <= self.x + 2 and
            self.y - 2 <= player.y and player.y <= self.y + 2 and
            self.z - 2 <= player.z and player.z <= self.z + 2 then
        return true
    end
    return false
end

function AgentMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        if player.has_zoomerator then
            player:ToastMessage("\x12 Trade \x0cZoomerator\x08 for \x0cSonic Summoner\x08", 1)
        else
            player:ToastMessage("Bring the prize from the hoverboard races", 1)
        end
    end
end

function AgentMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) and player.has_zoomerator then
        player:OnUnlockItem(Item.GetByName("Sonic Summoner").id, true)
    end
end

function AgentMoby:Disable()
    self.disabled = true
    self:Delete()
end 