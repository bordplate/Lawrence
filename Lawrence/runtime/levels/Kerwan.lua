require 'runtime.levels.Common.HostedByNearestPlayer'
require 'runtime.levels.Kerwan.Helga'

Kerwan = class("Kerwan", Level)

function Kerwan:initialize(internalEntity)
    Level.initialize(self, internalEntity)

    self:LoadHybrids()
end

function Kerwan:LoadHybrids()
    self.helga = Helga(self, 158)
end
