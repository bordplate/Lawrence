PlatformBooster = class("PlatformBooster", HybridMoby)

function PlatformBooster:initialize(level, uid, boltCrank)
    print("Initializing PlatformBooster")
    HybridMoby.initialize(self, level, uid)
    
    self.boltCrank = boltCrank
    
    self.activePlayer = null
    self.state = 1

    self:MonitorAttribute(Moby.offset.state, 1)
    self:MonitorAttribute(Moby.offset.animationFrameBlendT, 4, true)
end

function PlatformBooster:OnAttributeChange(player, offset, oldValue, newValue)
    if self.boltCrank.owningPlayer == null or player:GUID() ~= self.boltCrank.owningPlayer:GUID() then
        return
    end

    if offset == Moby.offset.state then
        self.state = newValue
    end
    
    if offset == Moby.offset.animationFrameBlendT then
        self:ChangeAttributeForOtherPlayers(player, Moby.offset.animationFrameBlendT, 4, newValue, true)
    end
end 

function PlatformBooster:OnTick()
    if self.activePlayer == null and self.boltCrank.owningPlayer ~= null then
        print("Setting active player for platform booster to " .. self.boltCrank.owningPlayer:Username())
        self.activePlayer = self.boltCrank.owningPlayer
        
        -- Set bogus state to prevent the native update function from interfering
        self:ChangeAttributeForOtherPlayers(self.activePlayer, Moby.offset.state, 1, 100)
    elseif self.activePlayer ~= null and self.boltCrank.owningPlayer == null then
        self:ChangeAttributeForOtherPlayers(self.activePlayer, Moby.offset.state, 1, self.state)
        self.activePlayer = null
    end
end