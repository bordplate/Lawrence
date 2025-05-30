BoltCrank = class("BoltCrank", HybridMoby)

function BoltCrank:initialize(level, uid)
    HybridMoby.initialize(self, level, uid)
    
    self:MonitorAttribute(Moby.offset.state, 1)
    self:MonitorAttribute(Moby.offset.rotation.z, 4, true)
    self:MonitorAttribute(Moby.offset.position.z, 4, true)
    self:MonitorPVar(0, 4, true)
    
    self.owningPlayer = null
    
    self.state = 0
end

function BoltCrank:OnAttributeChange(player, offset, oldValue, newValue)
    if self.owningPlayer ~= null and player:GUID() ~= self.owningPlayer:GUID() then
        if offset == Moby.offset.state then
            player:ChangeMobyAttribute(self.UID, Moby.offset.state, 1, 100)
        end
        
        return
    end
    
    if offset == Moby.offset.state then
        self.state = newValue
        print("BoltCrank state from " .. oldValue .. " changed to " .. newValue .. " by " .. player:Username())
        if newValue == 3 then
            self.owningPlayer = player
            
            print("Setting owning player to " .. player:Username())
            
            -- Set bogus state for other players, otherwise the bolt crank update function interferes 
            self:ChangeAttributeForOtherPlayers(player, Moby.offset.state, 1, 100)
        elseif newValue == 1 or newValue == 5 then
            self:ChangeAttributeForOtherPlayers(player, Moby.offset.state, 1, newValue)
        end
    end

    if offset == Moby.offset.rotation.z then
        self:ChangeAttributeForOtherPlayers(player, Moby.offset.rotation.z, 4, newValue, true)
    end
    
    if offset == Moby.offset.position.z then
        self:ChangeAttributeForOtherPlayers(player, Moby.offset.position.z, 4, newValue, true)
    end
end

function BoltCrank:OnPVarChange(player, offset, oldValue, newValue)
    if (self.owningPlayer == null or player:GUID() ~= self.owningPlayer:GUID()) then
        return
    end

    if newValue < oldValue and newValue == 0.0 then
        print("Setting owning player to nil")
        self.owningPlayer = null
    end
    
    self:ChangePVarForOtherPlayers(player, 0, 4, newValue, true)
end
