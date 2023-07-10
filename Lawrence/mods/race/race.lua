require 'RaceUniverse'
require 'Player'

----
-- Game mode functions
----

raceUni = nil

StartRaceObject = class("StartRaceObject", Moby)

function StartRaceObject:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    -- Crate
    self:SetOClass(556)

    self.rotationSpeed = 0.01*math.pi
    self.amplitude = 0.01  -- the height of the bouncing
    self.frequency = 0.001  -- how fast the bouncing occurs
    self.bounceZ = self.z

    self.alpha = 1
    self.scale = 0.2

    self.rotY = 0.0
end

function StartRaceObject:OnTick()
    self.rotZ = self.rotZ + self.rotationSpeed * Game:DeltaTime()  -- update rotation
    self.bounceZ = (self.amplitude * math.sin(self.frequency * Game:Time()))
    self.z = self.z + self.bounceZ  -- update z position
end

local LobbyPlayer = class("LobbyPlayer", Player)

function LobbyPlayer:Made()
    self.startRaceObject = nil
end

function LobbyPlayer:OnControllerInputTapped(input)
    if input & 16 ~= 0 and self.startRaceObject ~= nil and self:DistanceTo(self.startRaceObject) < 3 then
        raceUni:AddEntity(self)
    end
end

function LobbyPlayer:OnRespawned()
    
end

function LobbyPlayer:OnTick()
    -- Player is locked to state 100 when spawning
    if self.state == 100 then
        self.state = 0
    end
    
    if not self:IsWithinCube(
            301, 277, 117,
            321, 300, 125
    ) then
        self.x = 310
        self.y = 282
        self.z = 120
        
        self.state = 0
        
        self:DeleteAllChildrenWithOClass(1134)  -- Gold bolts
        self:DeleteAllChildrenWithOClass(1135)  -- Transporters
        self:DeleteAllChildrenWithOClass(315)   -- Transporter teeth

        if self.startRaceObject == nil then
            self.startRaceObject = self:SpawnInstanced(StartRaceObject)
            self.startRaceObject:SetPosition(304, 289, 119)
        end
    end

    if self.prev_state == nil then
        self.prev_state = self.state
    end

    if self.state ~= self.prev_state then
        self.prev_state = self.state
    end

    if self.startRaceObject ~= nil and self:DistanceTo(self.startRaceObject) < 3 then
        self:ToastMessage("\x12 Race")
    end
end

local LobbyUniverse = class("LobbyUniverse", Universe)
function LobbyUniverse:initialize()
    Universe.initialize(self)

    local raceUniverse = RaceUniverse:new()
    raceUniverse:Start(false)
    
    raceUni = raceUniverse

    self.races = { raceUniverse }
end

-- When a new player joins this Universe. 
function LobbyUniverse:OnPlayerJoin(player)
    player:LoadLevel("KaleboIII")
    player:Make(LobbyPlayer)
end

function LobbyUniverse:OnTick()
    local players = self:FindChildren("Player")

    if #players <= 0 then
        return
    end

    --for i, race in ipairs(self.races) do
    --    if race.playerCount < race.maxPlayers then
    --        for j, player in ipairs(players) do
    --            race:AddEntity(player)
    --            goto break_player_add
    --        end
    --
    --        ::break_player_add::
    --    end
    --end
end

local universe = LobbyUniverse:new()

-- Start the Universe as primary universe.
-- Primary universes handle player join notifications. 
universe:Start(true)