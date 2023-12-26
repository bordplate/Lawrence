---
--- Created by bordplate.
--- DateTime: 20/07/2023 21:35
---

MGBPlayer = class("MGBPlayer", Player)

-- TODO: Store items the player has gotten in case they need to reconnected
-- TODO: Store which planet the player is on in case they need to reconnected

function MGBPlayer:Made()
    self.damageCooldown = 0
    self.goldBoltCount = 0
end

function MGBPlayer:OnCollectedGoldBolt(planet, number)
    print("Player collected gold bolt on " .. planet .. " number: " .. number);
    
    self:Parent():BlockGoldBolt(planet, number)
    
    self.universe.blocked_bolts[#self.universe.blocked_bolts+1] = {planet, number}
    
    self.goldBoltCount = self.goldBoltCount + 1
end

function MGBPlayer:OnAttack(moby)
    if self.damageCooldown <= 0 then
        moby:Damage(1)
        self.damageCooldown = 40
    end
end

function MGBPlayer:Unfreeze()
    self.state = 0
end

function MGBPlayer:OnTick()
    if (self.damageCooldown > 0) then
        self.damageCooldown = self.damageCooldown - 1
    end
end