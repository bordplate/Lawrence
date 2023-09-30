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
    Player.OnUnlockItem(self, item) -- insert rando code here to change item
end