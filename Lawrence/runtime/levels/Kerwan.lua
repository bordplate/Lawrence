require 'runtime.levels.Common.HostedByNearestPlayer'

Kerwan = class("Kerwan", Level)

function Kerwan:initialize(internalEntity)
    Level.initialize(self, internalEntity)

    self:LoadHybrids()
end

function Kerwan:LoadHybrids()

end
