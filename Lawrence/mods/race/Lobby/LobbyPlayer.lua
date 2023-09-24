require 'Lobby.StartRaceMoby'


LobbyPlayer = class("LobbyPlayer", Player)

function LobbyPlayer:Made()
    self.startRaceMoby = nil
    
    self.waitingLabel = Label:new("Waiting for current race to end...", 250, 200, 0xC0FFA888)
    
    self.joinRace = nil
end

function LobbyPlayer:OnControllerInputTapped(input)
    if input & 16 ~= 0 and self.startRaceMoby ~= nil and self:DistanceTo(self.startRaceMoby) < 3 then
        local race = self:Universe():LuaEntity().races[1]

        -- We should only add the player to the race if it's currently in the "waiting" mode. 
        -- It's hard to manage players dynamically joining during a race, but ideally we'd have a "spectator mode" of sorts. 
        -- Instead we just let them play in the hoverboard race on Kalebo while they're waiting. 
        if race.mode == 1 then
            race:AddEntity(self)
        else
            self:AddLabel(self.waitingLabel)
            self.joinRace = race
            
            self:SetPosition(142, 90, 82)
        end
    end
end

function LobbyPlayer:OnRespawned()
end

function LobbyPlayer:OnTick()
    -- Player is locked to state 100 when spawning
    if self.state == 100 then
        self.state = 0
    end

    if self.joinRace ~= nil then
        if self.joinRace.mode == 1 then
            self:RemoveLabel(self.waitingLabel)
            
            self.joinRace:AddEntity(self)
            self.joinRace = nil
        end
    end
    
    if self.joinRace == nil and not self:IsWithinCube(
            301, 277, 117,
            321, 300, 125
    ) then
        self.x = 310
        self.y = 282
        self.z = 120

        self.state = 0

        -- Delete some of the mobys in the player's game that we don't want to have in the lobby room. 
        self:DeleteAllChildrenWithOClass(1134)  -- Gold bolts
        self:DeleteAllChildrenWithOClass(1135)  -- Transporters
        self:DeleteAllChildrenWithOClass(315)   -- Transporter teeth

        -- Spawn the moby that represents starting the race. 
        if self.startRaceMoby == nil then
            self.startRaceMoby = self:SpawnInstanced(StartRaceMoby)
            self.startRaceMoby:SetPosition(304, 289, 119)
        end
    end

    if self.prev_state == nil then
        self.prev_state = self.state
    end

    if self.state ~= self.prev_state then
        self.prev_state = self.state
    end

    -- Show a "start race" message when the player is within a certain distance of the "start race" moby. 
    if self.startRaceMoby ~= nil and self:DistanceTo(self.startRaceMoby) < 3 then
        self:ToastMessage("\x12 Join Race")
    end
end