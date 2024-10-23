require 'runtime.levels.Common.Elevator'

Umbris = class("Umbris", Level)

function Umbris:initialize(internalEntity)
    Level.initialize(self, internalEntity)

    self:LoadHybrids()
end

function Umbris:LoadHybrids()
    self.elevator = Elevator(self, 150)
end
