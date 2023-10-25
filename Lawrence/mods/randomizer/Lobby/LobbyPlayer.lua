require 'Lobby.StartRandoMoby'

LobbyPlayer = class("LobbyPlayer", Player)

function LobbyPlayer:Made()
    self.startCasualRandoMoby = nil
    self.startSpeedrunRandoMoby = nil

    
    self.waitingLabel = Label:new("Waiting for current race to end...", 250, 200, 0xC0FFA888)
    
    self.joinRando = nil
end

function LobbyPlayer:OnControllerInputTapped(input)
    if input & 16 ~= 0 and self.startCasualRandoMoby ~= nil and self:DistanceTo(self.startCasualRandoMoby) < 3 then
        local rando = self:Universe():LuaEntity().randos[1]
        rando:AddEntity(self)
    end
    if input & 16 ~= 0 and self.startSpeedrunRandoMoby ~= nil and self:DistanceTo(self.startSpeedrunRandoMoby) < 3 then
        local rando = self:Universe():LuaEntity().randos[1] -- idk if this should become another universe
        rando:AddEntity(self)
    end
    -- possibility for other configurations like in/excluding gold bolts and such
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

    -- print("x: " .. self.x .. " y: " .. self.y .. " z: " .. self.z)

    if self.joinRando == nil and not self:IsWithinCube(
        138, 105, 30,
        155, 80, 40
    ) then
        self.x = 146
        self.y = 96
        self.z = 33

        self:ToastMessage("Please stay in the lobby!", 100)

        -- self.state = 0

        -- -- Delete some of the mobys in the player's game that we don't want to have in the lobby room. 
        -- self:DeleteAllChildrenWithOClass(1134)  -- Gold bolts
        -- self:DeleteAllChildrenWithOClass(1135)  -- Transporters
        -- self:DeleteAllChildrenWithOClass(315)   -- Transporter teeth

        -- Spawn the moby that represents starting the race. 
        if self.startCasualRandoMoby == nil then
            self.startCasualRandoMoby = self:SpawnInstanced(startCasualRandoMoby)
            self.startCasualRandoMoby:SetPosition(146, 93, 33)
        end
        -- if self.startSpeedrunRandoMoby == nil then
        --     self.startSpeedrunRandoMoby = self:SpawnInstanced(startSpeedrunRandoMoby)
        --     self.startSpeedrunRandoMoby:SetPosition(310, 295, 119)
        -- end
    end


    -- Show a message when the player is within a certain distance to one of the starting Moby's
    if self.startCasualRandoMoby ~= nil and self:DistanceTo(self.startCasualRandoMoby) < 3 then
        self:ToastMessage("\x12 Start Casual Rando")
    end
    if self.startSpeedrunRandoMoby ~= nil and self:DistanceTo(self.startSpeedrunRandoMoby) < 3 then
        self:ToastMessage("\x12 Start Speedrun Rando")
    end
end