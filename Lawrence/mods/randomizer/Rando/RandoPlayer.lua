RandoPlayer = class("RandoPlayer", Player)

--function RandoPlayer:initialize(internalEntity)
    --self.timerLabel = Label:new("00:00.000", 420, 10, 0xC0FFA888)
--    Player.initialize(self, internalEntity)
--    print("RandoPlayer initialize")
--end

function RandoPlayer:Made()
    --self.startTime = Game:time()
    print("RandoPlayer made")
    self.randoUniverse = nil
end

function RandoPlayer:OnTick()
    --self.timerLabel:SetText(millisToTime(Game:Time() - self.startTime))
end

function RandoPlayer:OnUnlockItem(item)
    --Player.OnUnlockItem(self, item)
    -- instead of straight up unlocking, should ask self.randoUniverse.(map???)which item to give
    -- this way the Universe decides what items are given and what rules are used to do so
    -- which is independent from the player so all players in one universe have the same seed
    local randoItem = self.randoUniverse.itemMap[item]
    self:GiveItem(randoItem[1], randoItem[2]) -- the values in itemMap are pairs stating {item, equip} with equip deciding wether the item is equiped on unlock
end