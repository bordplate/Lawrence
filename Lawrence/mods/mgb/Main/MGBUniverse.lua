require 'Main.MGBPlayer'

MGBUniverse = class("MGBUniverse", Universe)

function MGBUniverse:initialize()
    Universe.initialize(self)
    
    self.blocked_bolts = {}
    
    self.players = {}
    self.playerLabels = {}
end

function MGBUniverse:OnPlayerJoin(player)
    player = player:Make(MGBPlayer)
    
    -- Find out if we already had the player and if they just disconnected and reconnected
    -- If they reconnected, we update their info and replace the old player
    for i, _player in ipairs(self.players) do
        if _player:Username() == player:Username() then
            print("Player " .. player:Username() .. " reconnected")
            
            player.goldBoltCount = _player.goldBoltCount
            
            -- TODO: Restore items and put player back on the level they were on before disconnecting
            -- However, we can assume that if a player is a on a planet other than Veldin1, they are still in game
            --   so we should only restore items and level if they are on Veldin1.
            
            -- temporarily just put on veldin1
            player:LoadLevel("Veldin1")
            
            self.players[i] = player
        end
    end

    for i, entry in ipairs(self.blocked_bolts) do
        print("Blocking " .. entry[1] .. " bolt: " .. entry[2])
        player:BlockGoldBolt(entry[1], entry[2])
    end
end

function MGBUniverse:StartMGB()
    print("Starting Most Gold Bolts")
    
    -- Set this universe as primary so players join into this one instead of the lobby if they DC
    self:SetPrimary(true)
    
    self.players = self:FindChildren("Player")

    -- Add labels for all players
    for i, player in ipairs(self.players) do
        print("Added player " .. player:Username() .. " to MGB")
        
        local playerLabel = Label:new(player:Username() .. ": " .. player.goldBoltCount, 0, 10 + (i-1) * 15, 0xC0FFA888)
        self:AddLabel(playerLabel)
        
        self.playerLabels[i] = playerLabel
        
        -- Set player position offset by amount of players
        player:SetPosition(135, 116 + (i-1) * 5, 33)
    end
    
    print("Starting countdown")
    
    -- Make countdown label
    self.countdown = 3 * 60  -- 3 seconds at 60 FPS
    self.countdownLabel = Label:new("", 250, 250, 0xC0FFA888)
    self:AddLabel(self.countdownLabel)
end

-- OnTick runs 60 times per second
function MGBUniverse:OnTick()
    if #self.players <= 0 then
        return
    end
    
    -- Update player labels with gold bolt count
    for i, player in ipairs(self.players) do
        self.playerLabels[i]:SetText(player:Username() .. ": " .. player.goldBoltCount)

        if self.countdown > 0 then
            player.state = 114
        end
    end
    
    -- Update countdown only once every second
    if self.countdown > 0 and self.countdown % 60 == 0 then
        self.countdownLabel:SetText("" .. math.floor(self.countdown / 60))
    elseif self.countdown < 0 and self.countdown > -60 then
        self.countdownLabel:SetText("GO!")
        
        -- Unfreeze players
        for i, player in ipairs(self.players) do
            if player.state == 114 then
                player:Unfreeze()
            end
        end
    end
    
    if self.countdown < -60 then
        self:RemoveLabel(self.countdownLabel)
    else
        self.countdown = self.countdown - 1
    end
end 