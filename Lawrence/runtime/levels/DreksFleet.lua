require 'runtime.levels.Common.Button'

DreksFleet = class("DreksFleet", Level)

function DreksFleet:initialize(internalEntity)
    Level.initialize(self, internalEntity)

    self:LoadHybrids()
end

function DreksFleet:LoadHybrids()
    self.button1 = Button(self, 3)
end
