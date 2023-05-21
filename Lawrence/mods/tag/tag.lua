----
-- Game mode functions
----

require 'HunterPlayer'
require 'TagPlayer'
require 'Pickup'

pickups = {
    {x=230, y=166, z=55.5, oclass=500, item=2, respawn=0, moby=nil},
    {x=230, y=170, z=55.5, oclass=500, item=3, respawn=0, moby=nil}
}

local TagUniverse = class("TagUniverse", Universe)
function TagUniverse:initialize()
    Universe.initialize(self)
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
        if pickup.respawn <= 0 and (pickup.moby == nil or pickup.moby:IsDeleted()) then
            local moby = self:GetLevelByName("Eudora"):SpawnMoby(500):Make(Pickup)
            moby:SetPosition(pickup.x, pickup.y, pickup.z)
            moby.item = pickup.item

            pickup.moby = moby
            pickup.respawn = 120  -- Respawn every 2 seconds

            print("Spawned pickup for item: " .. pickup.item)
        end

        -- Decrease respawn timer if the item has been picked up
        if pickup.moby == nil or pickup.moby:IsDeleted() then
            pickup.respawn = pickup.respawn - 1
        end
    end
end

local universe = TagUniverse:new()
universe:Start(true)