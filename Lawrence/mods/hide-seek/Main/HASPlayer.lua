---
--- Created by bordplate.
--- DateTime: 20/07/2023 21:35
---

HASPlayer = class("HASPlayer", Player)

-- TODO: Store items the player has gotten in case they need to reconnected
-- TODO: Store which planet the player is on in case they need to reconnected

DEFAULT_ITEMS = {
    2, 3, 4, 6,  28, 29, 12, 32, 11, 25
}

function HASPlayer:Made()
    self.damageCooldown = 0
    self.seeker = false
    
    self.statusLabel = Label:new("Hider", 80, 50, 0xC0FFA888)
    self.timerLabel = Label:new("00:00", 80, 70, 0xC0FFA888)
    self.foundTimerLabel = Label:new("", 80, 90, 0xC0FFFF88)
    
    self.startTime = Game:Time()
    self.endTime = 0
    self.foundTime = 0
    
    self.started = false
    
    self:AddLabel(self.statusLabel)
    self:AddLabel(self.timerLabel)

    -- Give all items
    for i, item in ipairs(DEFAULT_ITEMS) do
        self:GiveItem(item)
    end
    
    self.respawned = 0
    
    self:SetColor(0, 255, 0)
end

function HASPlayer:StartGame()
    self.started = true
    self.startTime = Game:Time()
end

function HASPlayer:MakeSeeker()
    self.endTime = Game:Time()
    
    self.seeker = true
    
    self:SetColor(255, 0, 0)
    
    self.statusLabel:SetText("Seeker")
end

function HASPlayer:Found()
    print("Player " .. self:Username() .. " has become seeker")
    
    self.foundTime = Game:Time()
    
    self.foundTimerLabel:SetText(millisToTimeSeconds(self.foundTime - self.startTime))
    self:AddLabel(self.foundTimerLabel)
    
    self:Damage(8)
    self:MakeSeeker()
end

function HASPlayer:Finished()
    self:RemoveLabel(self.statusLabel)
    self:RemoveLabel(self.timerLabel)
    self:RemoveLabel(self.foundTimerLabel)
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
    if self.respawned > 1 then
        self:MakeSeeker()
    end
    
    self.respawned = self.respawned + 1
end

function HASPlayer:Unfreeze()
    self.state = 0
end

function HASPlayer:OnTick()
    if self.started then
        self.timerLabel:SetText(millisToTimeSeconds(Game:Time() - self.startTime))
    end
    
    if (self.damageCooldown > 0) then
        self.damageCooldown = self.damageCooldown - 1
    end
end
