---
--- Created by bordplate.
--- DateTime: 21/05/2023 16:09
---

Pickup = class('Pickup', Moby)

function Pickup:Made()
    self:SetOClass(500)
    self.scale = 0.012
end

function Pickup:OnCollision(moby)
    -- We only care about players, so we return to stop processing further if we've just collided with 
    --  something that isn't a player. 
    if not moby:Is(Player) then
        return
    end
    
    moby:GiveItem(self.item)
    self:Delete()
end 