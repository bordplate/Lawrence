---
--- Created by bordplate.
--- DateTime: 20/07/2023 21:35
---

MGBPlayer = class("MGBPlayer", Player)
function MGBPlayer:Made()
    self:GiveItem(2)
    self:GiveItem(3)
    
    self.damageCooldown = 0
end

function MGBPlayer:SetUniverse(universe)
    self.universe = universe
end

function MGBPlayer:OnCollectedGoldBolt(planet, number)
    print("Player collected gold bolt on " .. planet .. " number: " .. number);
    
    self:Parent():BlockGoldBolt(planet, number)
    
    self.universe.blocked_bolts[#self.universe.blocked_bolts+1] = {planet, number}
end

function MGBPlayer:OnAttack(moby)
    --if moby:Is(TagPlayer) then
    if self.damageCooldown <= 0 then
        print("Attacked")
        moby:Damage(1)
        self.damageCooldown = 40
    end
end

function MGBPlayer:OnTick()
    if (self.damageCooldown > 0) then
        self.damageCooldown = self.damageCooldown - 1
    end
end 