HelgaMoby = class("HelgaMoby", Moby)

function HelgaMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(890)
    self:SetPosition(117.218, 83.312, 65.833)
    self.rotZ = 1.787
    
    self.scale = 0.2
end

function HelgaMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
       self.x - 2 <= player.x and player.x <= self.x + 2 and
       self.y - 2 <= player.y and player.y <= self.y + 2 and 
       self.z - 2 <= player.z and player.z <= self.z + 2 then
           return true
    end
   return false
end
