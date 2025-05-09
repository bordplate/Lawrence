require 'runtime.levels.BlargStation.SpaceShuttle'
require 'runtime.levels.BlargStation.RedButton'
require 'runtime.levels.BlargStation.AlienSnapper'

BlargStation = class("BlargStation", Level)

function BlargStation:initialize(internalEntity)
    Level.initialize(self, internalEntity)

    self:LoadHybrids()
end

function BlargStation:LoadHybrids()
    self.spaceShuttle = SpaceShuttle(self, 158)
    self.redButton = RedButton(self, 159)
    
    self.snapper1 = AlienSnapper(self, 68)
    self.snapper2 = AlienSnapper(self, 69)
    self.snapper3 = AlienSnapper(self, 71)
end
