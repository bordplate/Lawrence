LobbyPlayer = class("LobbyPlayer", Player)

function LobbyPlayer:Made()
    self.ready = false
    
    self.readyMoby = self:Universe():LuaEntity().readyMoby
end

function LobbyPlayer:OnControllerInputTapped(input)
    if input & 16 ~= 0 and not self.ready and self:DistanceTo(self.readyMoby) < 3 then
        self.ready = true
    end
end

function LobbyPlayer:OnRespawned()
end

function LobbyPlayer:OnUnlockItem(item)
end

function LobbyPlayer:OnTick()
    -- Player is locked to state 100 when spawning
    if self.state == 100 then
        self.state = 0
    end

    if self.prev_state == nil then
        self.prev_state = self.state
    end

    if self.state ~= self.prev_state then
        self.prev_state = self.state
    end

    if not self:IsWithinCube(
        301, 277, 117,
        321, 300, 125
    ) then
        self.x = 310
        self.y = 282
        self.z = 120

        -- Delete some of the mobys in the player's game that we don't want to have in the lobby room. 
        self:DeleteAllChildrenWithOClass(1134)  -- Gold bolts
        self:DeleteAllChildrenWithOClass(1135)  -- Transporters
        self:DeleteAllChildrenWithOClass(315)   -- Transporter teeth

        self:ToastMessage("Please stay in the lobby!", 100)
    end
    
    if not self.ready and self:DistanceTo(self.readyMoby) < 3 then
        self:ToastMessage("\x12 Ready to start")
    end
end