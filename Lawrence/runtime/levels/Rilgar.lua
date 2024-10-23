require 'runtime.levels.Common.Button'

Rilgar = class("Rilgar", Level)

function Rilgar:initialize(internalEntity)
    Level.initialize(self, internalEntity)

    self:LoadHybrids()
end

function Rilgar:LoadHybrids()
    self.button1 = Button(self, 25)
    self.button2 = Button(self, 26)
    self.button3 = Button(self, 130)
    self.button4 = Button(self, 294)
end
