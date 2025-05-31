TrespasserMoby = class("TrespasserMoby", Moby)

function TrespasserMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(1005)
    self:SetPosition(98, 274, 61.2)
    self.rotX = 1.571
    self.rotY = 3.142
    self.rotZ = -0.033

    self.scale = 0.044

    self.rotationSpeed = 0.01*math.pi

    self.disabled = false
end

function TrespasserMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 2 <= player.x and player.x <= self.x + 2 and
            self.y - 2 <= player.y and player.y <= self.y + 2 and
            self.z - 2 <= player.z and player.z <= self.z + 2 then
        return true
    end
    return false
end

function TrespasserMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        player:ToastMessage("\x12 Take \x0cTresspasser\x08", 1)
    end
end

function TrespasserMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) then
        player:OnUnlockItem(Item.GetByName("Trespasser").id, true)
    end
end

function TrespasserMoby:Disable()
    self.disabled = true
    self:Delete()
end

function TrespasserMoby:OnTick()
    self.rotZ = self.rotZ + self.rotationSpeed * Game:DeltaTime()  -- update rotation
end