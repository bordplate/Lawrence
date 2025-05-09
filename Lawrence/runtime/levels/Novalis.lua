require 'runtime.levels.Novalis.ShipRear'
require 'runtime.levels.Common.HostedByNearestPlayer'

Novalis = class("Novalis", Level)

function Novalis:initialize(internalEntity)
    Level.initialize(self, internalEntity)

    self:LoadHybrids()
end

function Novalis:LoadHybrids()
    self.shipRear = ShipRear(self, 108)
end
