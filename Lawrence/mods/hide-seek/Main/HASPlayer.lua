---
--- Created by bordplate.
--- DateTime: 20/07/2023 21:35
---

HASPlayer = class("HASPlayer", Player)

-- TODO: Store items the player has gotten in case they need to reconnected
-- TODO: Store which planet the player is on in case they need to reconnected

DEFAULT_ITEMS = {
    2, 3, 4, 6,  28, 29, 12, 32, 11, 14, 25
}

function HASPlayer:Made()
    self.damageCooldown = 0
    self.seeker = false
    
    self.statusLabel = Label:new("Hider", 0, 0, 0xC0FFA888)
    self:AddLabel(self.statusLabel)

    -- Give all items
    for i, item in ipairs(DEFAULT_ITEMS) do
        self:GiveItem(item)
    end
end

function HASPlayer:MakeSeeker()
    self.seeker = true
    
    self.statusLabel:SetText("Seeker")
end

function HASPlayer:Found()
    self:Damage(8)
    self:MakeSeeker()
end

function HASPlayer:Finished()
    self:RemoveLabel(self.statusLabel)
end

function HASPlayer:OnAttack(moby)
    if self.damageCooldown <= 0 then
        if self.seeker and moby:Is(HASPlayer) then
            if not moby.seeker then
                moby:Found()
            end
        end
        
        self.damageCooldown = 40
    end
end

function HASPlayer:OnRespawned()
    self:MakeSeeker()
end

function HASPlayer:Unfreeze()
    self.state = 0
end

function HASPlayer:OnTick()
    if (self.damageCooldown > 0) then
        self.damageCooldown = self.damageCooldown - 1
    end
end
