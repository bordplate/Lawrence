----
-- Game mode functions
----
local SandboxUniverse = class("TagUniverse", Universe)
function SandboxUniverse:initialize()
    Universe.initialize(self)
end

-- When a new player joins this Universe. 
function SandboxUniverse:OnPlayerJoin(player)
    player:LoadLevel("Veldin1")
end

function SandboxUniverse:OnTick()

end

local universe = SandboxUniverse:new()
universe:Start(true)