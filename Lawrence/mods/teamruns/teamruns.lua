require 'TeamRunPlayer'

local TeamRunsUniverse = class("TeamRunsUniverse", Universe)
function TeamRunsUniverse:initialize()
    Universe.initialize(self)
end
 
function TeamRunsUniverse:OnPlayerJoin(player)
    player = player:Make(TeamRunPlayer)
    
    player:GiveItem(Item.GetByName("Heli-pack").id)
    player:GiveItem(Item.GetByName("Thruster-pack").id)
    player:GiveItem(Item.GetByName("Hydro-pack").id)
    player:GiveItem(Item.GetByName("O2 Mask").id)
    player:GiveItem(Item.GetByName("Pilot's Helmet").id)
    player:GiveItem(Item.GetByName("PDA").id)
    player:GiveItem(Item.GetByName("Pyrociter").id)
    player:GiveItem(Item.GetByName("Swingshot").id)
    player:GiveItem(Item.GetByName("Devastator").id)
    player:GiveItem(Item.GetByName("Visibomb").id)
    player:GiveItem(Item.GetByName("Hologuise").id)
    player:GiveItem(Item.GetByName("R.Y.N.O.").id)
    
    player:SetBolts(150000)
    player:LoadLevel("Kerwan")
end

function TeamRunsUniverse:OnTick()
    
end

local universe = TeamRunsUniverse:new()
universe:Start(true)