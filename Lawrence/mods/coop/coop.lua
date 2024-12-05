require 'CoopPlayer'

local CoopUniverse = class("CoopUniverse", Universe)
function CoopUniverse:initialize()
    Universe.initialize(self)
end
 
function CoopUniverse:OnPlayerJoin(player)
    player = player:Make(CoopPlayer)
    
    player:GiveItem(Item.GetByName("Heli-pack").id)
    player:GiveItem(Item.GetByName("Thruster-pack").id)
    player:GiveItem(Item.GetByName("Hydro-pack").id)
    player:GiveItem(Item.GetByName("O2 Mask").id)
    player:GiveItem(Item.GetByName("Pilot's Helmet").id)
    player:GiveItem(Item.GetByName("PDA").id)
    player:GiveItem(Item.GetByName("Magneboots").id)
    player:GiveItem(Item.GetByName("Grindboots").id)
    player:GiveItem(Item.GetByName("Drone Device").id)
    player:GiveItem(Item.GetByName("Decoy Glove").id)
    player:GiveItem(Item.GetByName("Swingshot").id)
    player:GiveItem(Item.GetByName("Devastator").id)
    player:GiveItem(Item.GetByName("Sonic Summoner").id)
    player:GiveItem(Item.GetByName("Visibomb").id)
    player:GiveItem(Item.GetByName("Hologuise").id)
    player:GiveItem(Item.GetByName("R.Y.N.O.").id)
    player:GiveItem(Item.GetByName("Blaster").id)
    
    player:SetBolts(150000)
    player:LoadLevel("KaleboIII")
end

function CoopUniverse:OnTick()
    
end

local universe = CoopUniverse:new()
universe:Start(true)