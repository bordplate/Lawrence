----
-- Game mode functions
----

hunter_player_id = -1

pickups = {
    {x=230, y=166, z=55.5, oclass=500, item=2, respawn=0, moby=nil},
    {x=230, y=170, z=55.5, oclass=500, item=3, respawn=0, moby=nil}
}

local TagPlayer = class('TagPlayer', Player)

function TagPlayer:Made()
    
end

function TagPlayer:OnTick()
    
end

local HunterPlayer = class('HunterPlayer', Player)

function HunterPlayer:Made()
    self.hunterLabel = Label:new("Hunter", 60, 60, 0xff0000ff)
    self.ticksLabel = Label:new(self:Ticks() .. " player ticks", 400, 400, 0xC0FFA888)
    
    self:AddLabel(self.hunterLabel)
    self:AddLabel(self.ticksLabel)
end

function HunterPlayer:OnTick()
    self.ticksLabel:SetText(self:Ticks() .. " player ticks")

    if (self:Ticks() % 60) == 0 then
        self:RemoveLabel(self.hunterLabel)
    elseif (self:Ticks() % 60) == 30 then
        self:AddLabel(self.hunterLabel)
    end

    if self:Ticks() > 1000 then
        self:RemoveLabel(self.ticksLabel)
    end
end
    
local TagUniverse = class("TagUniverse", Universe)
function TagUniverse:initialize()
    Universe.initialize(self)

    self.hunter = nil
    
    self:SpawnPickups()
end

function TagUniverse:SpawnPickups()
    
end

-- When a new player joins this Universe. 
function TagUniverse:OnPlayerJoin(player)
    player:LoadLevel("Eudora")

    if self.hunter ~= nil then
        self.hunter:Make(TagPlayer)
    end
    
    player = player:Make(HunterPlayer)
    self.hunter = player
end

function TagUniverse:OnTick()
    -- Go through pickups and respawn if necessary
    for i, pickup in ipairs(pickups) do
        if pickup.respawn <= 0 and pickup.moby == nil then
            local moby = self:GetLevelByName("Eudora"):SpawnMoby(500)
            moby:SetPosition(pickup.x, pickup.y, pickup.z)

            pickup.moby = moby
            pickup.respawn = 120  -- Respawn every 2 seconds

            print("Spawned pickup for item: " .. pickup.item)
        end

        -- Decrease respawn timer if the item has been picked up
        if pickup.moby == nil then
            pickup.respawn = pickup.respawn - 1
        end
    end
end

local universe = TagUniverse:new()
universe:Start(true)


----
-- Engine callbacks
----

-- Called when the script is loaded
function on_load()

end

-- When a player connects
function on_player_connect(player)
    -- Send player to Eudora
    Environment:SendPlayerToPlanet(player.ID, 4)

    -- Last player to connect automatically becomes hunter
    hunter_player_id = player.ID
end

-- When player disconnects
function on_player_disconnect(player)
    
end

-- Called when a player collides with a game object
-- Flags are either 0 or 1, if the collision is passive or aggressive (attack), respectively
function on_collision(collider, collidee, flags)
    -- If the collidee has a "parent", that means this is a player object
    if collidee.parent ~= nil then
        -- Check for aggressive collision
        if flags > 0 then
            -- If the attacking player is the hunter
            if collider.parent.ID == hunter_player_id then
                -- Hunter is now the player who was hit
                hunter_player_id = collidee.parent.ID

                collidee:Damage(0)
            end
        end
    else -- Probably a pickup item
        print("Collided with thing")
        -- Check if we collided with a pickup
        for i, pickup in ipairs(pickups) do
            if pickup.moby ~= nil then
                if collidee.UUID == pickup.moby.UUID then
                    Environment:DeleteMoby(collidee.UUID)
                    Environment:GiveItemToPlayer(collider.parent.ID, pickup.item)

                    pickup.moby = nil
                end
            end
        end
    end
end

function on_collision_end(collider, collidee)

end

function on_player_input(player, input)

end

-- Called every tick
function tick()
    -- Go through pickups and respawn if necessary
    for i, pickup in ipairs(pickups) do
        if pickup.respawn <= 0 and pickup.moby == nil then
            local moby = Environment:SpawnMoby(pickup.oclass)
            moby.x = pickup.x
            moby.y = pickup.y
            moby.z = pickup.z
            moby.level = 4
            moby.active = true

            pickup.moby = moby

            pickup.respawn = 120  -- Respawn every 2 seconds

            print("Spawned pickup for item: " .. pickup.item)
        end

        -- Decrease respawn timer if the item has been picked up
        if pickup.moby == nil then
            pickup.respawn = pickup.respawn - 1
        end
    end
end

-- Called every tick for every player in the game
function player_tick(player)
    -- Write "Hunter" or "Runner" in the corner for the player, depending on if they are or not
    if hunter_player_id == player.ID then
        Environment:DrawTextForPlayer(player.ID, 1, "Hunter", 60, 60, 0xff0000ff)
    else
        Environment:DrawTextForPlayer(player.ID, 1, "Runner", 60, 60, 0xffff0000)
    end

    -- Draw position for player, for debugging
    Environment:DrawTextForPlayer(player.ID, 14, "x: " .. player:GetMoby().x, 400, 360, 0xff9e26e4)
    Environment:DrawTextForPlayer(player.ID, 13, "y: " .. player:GetMoby().y, 400, 380, 0xff9e26e4)
    Environment:DrawTextForPlayer(player.ID, 12, "z: " .. player:GetMoby().z, 400, 400, 0xff9e26e4)
end