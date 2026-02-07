ElevatorTest = class('ElevatorTest', Moby)

function ElevatorTest:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(726)
    self.MovablePlatform = true
    self.modeBits = 0x20
    self.scale = 0.2
    
    self.direction = true
    --self.ShouldRefreshPeriodically = false
end

function ElevatorTest:OnTick()
    if self.y > 160 then
        self.direction = false
    end
    if self.y < 145 then
        self.direction = true
    end

    if self.direction then
        self.y = self.y + 0.05
        self.z = self.z + 0.05
    else
        self.y = self.y - 0.05
        self.z = self.z - 0.05
    end
end 

function ElevatorTest:OnStandingPlayer(player)
    print("Player's standing on us!")
end

function ElevatorTest:OnRemovedStandingPlayer(player)
    print("Player's off us!")
end
