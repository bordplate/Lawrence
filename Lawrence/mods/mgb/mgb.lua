require 'MGBPlayer'

local MGBUniverse = class("MGBUniverse", Universe)
function MGBUniverse:initialize()
    Universe.initialize(self)
    
    self.blocked_bolts = {}
end

function MGBUniverse:OnPlayerJoin(player)
    player:LoadLevel("Novalis")
    player = player:Make(MGBPlayer)
    player:SetUniverse(self)

    for i, entry in ipairs(self.blocked_bolts) do
        print("Blocking " .. entry[1] .. " bolt: " .. entry[2])
        player:BlockGoldBolt(entry[1], entry[2])
    end
end

local universe = MGBUniverse:new()
universe:Start(true)