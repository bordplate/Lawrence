function RemoveReplacedMobys(player)
    if player:Level():GetName() == "Kerwan" then
        player:DeleteAllChildrenWithUID(158) -- Helga
        player:DeleteAllChildrenWithUID(165) -- Al
    end

    if player:Level():GetName() == "Pokitaru" then
        player:DeleteAllChildrenWithUID(653) -- Bob
    end
end

-- Helga
HelgaMoby = class("HelgaMoby", Moby)

function HelgaMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(890)
    self:SetPosition(117.218, 83.312, 65.833)
    self.rotZ = 1.787
    
    self.scale = 0.2
    
    self.disabled = false
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

function HelgaMoby:toastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        if player.totalBolts >= 1000 then
            player:ToastMessage("\x12 Buy \x0cSwingshot\x08 for 1,000 bolts ", 1)
        else
            player:ToastMessage("You need 1,000 bolts for the \x0cSwingshot\x08", 1)
        end
    end
end

function HelgaMoby:Triangle(player, universe) -- returns true if moby needs to be removed
    if not self.disabled and self:closeToPlayer(player) and player.totalBolts >= 1000 then
        player:GiveBolts(-1000)
        player:OnUnlockItem(Item.GetByName("Swingshot").id, true)
        self.disabled = true
        return true
    end
    return false
end

-- Bob
BobMoby = class("BobMoby",  Moby)

function BobMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(90)
    self:SetPosition(588, 579, 234)
    self.rotZ = 1.787
    
    self.scale = 0.2
    
    self.disabled = false
end

function BobMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
       self.x - 5 <= player.x and player.x <= self.x + 5 and
       self.y - 5 <= player.y and player.y <= self.y + 5 and 
       self.z - 5 <= player.z and player.z <= self.z + 5 then
           return true
    end
   return false
end

function BobMoby:toastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        if player.totalBolts >= 2000 then
            player:ToastMessage("\x12 Pay 2,000 Bolts for the \x0cThruster-Pack\x08", 1)
        else
            player:ToastMessage("You need 2,000 Bolts for the \x0cThruster-Pack\x08", 1)
        end
    end
end

function BobMoby:Triangle(player, universe) -- returns true if moby needs to be removed
    if not self.disabled and self:closeToPlayer(player) and player.totalBolts >= 2000 then
        player:GiveBolts(-2000)
        player:OnUnlockItem(Item.GetByName("Thruster-pack").id, true)
        self.disabled = true
    end
    return false
end

-- Al
AlMoby = class("AlMoby", Moby)

function AlMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(909)
    self:SetPosition(295, 240, 34)
    self.rotZ = 1.571
    
    self.scale = 0.2
    
    self.disabled = false
end

function AlMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
       self.x - 5 <= player.x and player.x <= self.x + 5 and
       self.y - 5 <= player.y and player.y <= self.y + 5 and 
       self.z - 5 <= player.z and player.z <= self.z + 5 then
           return true
    end
   return false
end

function AlMoby:toastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        if player.totalBolts >= 1000 then
            player:ToastMessage("\x12 Buy \x0cHeli-Pack\x08 for 1,000 bolts", 1)
        else
            player:ToastMessage("You need 1,000 Bolts for the \x0cHeli-Pack\x08", 1)
        end
    end
end

function AlMoby:Triangle(player, universe) -- returns true if moby needs to be removed
    if not self.disabled and self:closeToPlayer(player) and player.totalBolts >= 2000 then
        universe:DistributeGiveBolts(-1000)
        player:OnUnlockItem(Item.GetByName("Heli-pack").id, true)
        universe:DistributeSetLevelFlags(2, 3, 78, {[1]=1})
        self.disabled = true
    end
end