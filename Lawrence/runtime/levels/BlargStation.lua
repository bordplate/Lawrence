require 'runtime.levels.BlargStation.SpaceShuttle'

BlargStation = class("BlargStation", Level)

function BlargStation:initialize(internalEntity)
    Level.initialize(self, internalEntity)

    self:LoadHybrids()
end

function BlargStation:LoadHybrids()
    self.spaceShuttle = SpaceShuttle(self, 158)
end
