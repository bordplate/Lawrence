require 'runtime.levels.Orxon.HovenInfobot'
require 'runtime.levels.Orxon.Clank'

Orxon = class("Orxon", Level)

function Orxon:initialize(internalEntity)
    Level.initialize(self, internalEntity)

    self:LoadHybrids()
end

function Orxon:LoadHybrids()
    self.hovenInfobot = HovenInfobot(self, 256)
    self.clank = Clank(self, 77)
end
