require 'Rando.RandoPlayer'
require 'item_table_ids'

RandoUniverse = class("RandoUniverse", Universe) -- should probably have subclasses of RandoUniverse that decide more specific behaviors (such as what items can be achieved)
                                                 -- or just change up the universe based on choice
function RandoUniverse:initialize()
    Universe.initialize(self)

    self.maxPlayers = 8
    self.playerCount = 0

    self.itemMap = nil

    -- generate a randomisation map
    self:generateItemMap()
    -- for k,v in pairs(keyid) do
    --     print("pair: " .. k .. ": " .. v)
    -- end
end

function RandoUniverse:OnPlayerJoin(player)
    player = player:Make(RandoPlayer)
    player.randoUniverse = self
    --player:LoadLevel("Veldin1")
    player:OnUnlockItem(0xa) -- bomb glove
    player:SetBolts(150000)
    -- player:GiveBolts(5)
end

function RandoUniverse:generateItemMap()
    local itemList = {}
    local randomItemList = {}
    for k,v in pairs(keyid) do -- create the list of all items twice
        table.insert(itemList, v)
        table.insert(randomItemList, v)
    end

    shuffle(randomItemList) -- for now do a pure shuffle with no running logic

    local n = 0
    self.itemMap = {}

    for k in pairs(itemList) do -- create a Map, mapping all items to a randomly selected item. The mapped value is the item actually given to the Player
        n=n+1
        self.itemMap[k] = randomItemList[n]
    end
    
end

function shuffle(tbl)
    for i = #tbl, 2, -1 do
      local j = math.random(i)
      tbl[i], tbl[j] = tbl[j], tbl[i]
    end
  end