HostedByNearestPlayer = class("HostedByNearestPlayer", HybridMoby)

function HostedByNearestPlayer:initialize(level, uid, syncedPVars)
    syncedPVars = syncedPVars or {}
    
    print("Initializing HostedByNearestPlayer")
    HybridMoby.initialize(self, level, uid)

    self:MonitorAttribute(Moby.offset.state, 1)
    self:MonitorAttribute(Moby.offset.animationID, 1)
    self:MonitorAttribute(Moby.offset.position.x, 4, true)
    self:MonitorAttribute(Moby.offset.position.y, 4, true)
    self:MonitorAttribute(Moby.offset.position.z, 4, true)
    self:MonitorAttribute(Moby.offset.rotation.x, 4, true)
    self:MonitorAttribute(Moby.offset.rotation.y, 4, true)
    self:MonitorAttribute(Moby.offset.rotation.z, 4, true)
    
    for _, pVar in pairs(syncedPVars) do
        print("Doing pVar " .. pVar[1] .. " for " .. self.UID)
        self:MonitorPVar(pVar[1], pVar[2])
    end

    self.nearestPlayer = null
end

function HostedByNearestPlayer:OnAttributeChange(player, offset, oldValue, newValue)
    if self.nearestPlayer == null or player:GUID() ~= self.nearestPlayer:GUID() then
        player:ChangeMobyAttribute(self.UID, Moby.offset.modeBits, 2, bit.bor(32, 0x2))
        return
    end

    if offset == Moby.offset.state then
        print("HostedByNearestPlayer state from " .. oldValue .. " changed to " .. newValue .. " by " .. player:Username())

        if newValue ~= 0 then
            self:ChangeAttributeForOtherPlayers(player, Moby.offset.state, 1, newValue)
        end
    end
    
    if offset == Moby.offset.animationID then
        self:ChangeAttributeForOtherPlayers(player, Moby.offset.animationID, 1, newValue)
    end
    
    if offset == Moby.offset.position.x then
        self:SetPosition(newValue, self.y, self.z)
        self:ChangeAttributeForOtherPlayers(player, Moby.offset.position.x, 4, newValue, true)
    end
    if offset == Moby.offset.position.y then
        self:SetPosition(self.x, newValue, self.z)
        self:ChangeAttributeForOtherPlayers(player, Moby.offset.position.y, 4, newValue, true)
    end
    if offset == Moby.offset.position.z then
        self:SetPosition(self.x, self.y, newValue)
        self:ChangeAttributeForOtherPlayers(player, Moby.offset.position.z, 4, newValue, true)
    end

    if offset == Moby.offset.rotation.x then
        self:ChangeAttributeForOtherPlayers(player, Moby.offset.rotation.x, 4, newValue, true)
    end
    if offset == Moby.offset.rotation.y then
        self:ChangeAttributeForOtherPlayers(player, Moby.offset.rotation.y, 4, newValue, true)
    end
    if offset == Moby.offset.rotation.z then
        self:ChangeAttributeForOtherPlayers(player, Moby.offset.rotation.z, 4, newValue, true)
    end
end

function HostedByNearestPlayer:OnPVarChange(player, offset, oldValue, newValue)
    if self.nearestPlayer == null or player:GUID() ~= self.nearestPlayer:GUID() then
        return
    end
    
    self:ChangePVarForOtherPlayers(player, offset, 4, newValue, true)
end

function HostedByNearestPlayer:OnTick()
    local changed = false
    
    for _, player in pairs(self:Level():FindChildren("Player")) do
        if self.nearestPlayer == null then
            self.nearestPlayer = player
            changed = true
        end
        
        if self.nearestPlayer:GUID() ~= player:GUID() and self:DistanceTo(player) < self:DistanceTo(self.nearestPlayer) then
            self.nearestPlayer = player
            changed = true
        end
    end
    
    if changed then
        print("Nearest player to " .. self.UID .. " is now " .. self.nearestPlayer:Username())
        self.nearestPlayer:ChangeMobyAttribute(self.UID, Moby.offset.modeBits, 2, 32)
    end
    
    if self.nearestPlayer ~= null then
        self:ChangeAttributeForOtherPlayers(self.nearestPlayer, Moby.offset.modeBits, 2, bit.bor(32, 0x2))
    end
end
