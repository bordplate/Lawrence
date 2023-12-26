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
        138, 105, 30,
        155, 80, 40
    ) then
        self.x = 146
        self.y = 96
        self.z = 33

        self:ToastMessage("Please stay in the lobby!", 100)
    end
    
    if not self.ready and self:DistanceTo(self.readyMoby) < 3 then
        self:ToastMessage("\x12 Ready to start")
    end
end