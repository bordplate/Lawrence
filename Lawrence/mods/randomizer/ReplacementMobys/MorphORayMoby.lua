MorphORayMoby = class("MorphORayMoby", Moby)

function MorphORayMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(1354)
    self:SetPosition(310.824, 102.229, 69.534)
    self.rotZ = 1.787

    self.scale = 0.083

    self.rotationSpeed = 0.01*math.pi

    self.disabled = false
end

function MorphORayMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 2 <= player.x and player.x <= self.x + 2 and
            self.y - 2 <= player.y and player.y <= self.y + 2 and
            self.z - 2 <= player.z and player.z <= self.z + 2 then
        return true
    end
    return false
end

function MorphORayMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        player:ToastMessage("\x12 Get \x0cMorph-o-Ray\x08", 1)
    end
end

function MorphORayMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) then
        player:OnUnlockItem(Item.GetByName("Morph-o-Ray").id, true)
    end
end

function MorphORayMoby:Disable()
    self.disabled = true
    self:Delete()
end

function MorphORayMoby:OnTick()
    self.rotZ = self.rotZ + self.rotationSpeed * Game:DeltaTime()  -- update rotation
end