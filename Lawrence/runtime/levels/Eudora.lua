require 'runtime.levels.Eudora.BoltCrank'
require 'runtime.levels.Eudora.BridgePart'
require 'runtime.levels.Eudora.PlatformBooster'

Eudora = class("Eudora", Level)

function Eudora:initialize(internalEntity)
    Level.initialize(self, internalEntity)
    
    self:LoadHybrids()
end

function Eudora:LoadHybrids()
    self.boltCrank1 = BoltCrank(self, 45)
    self.bridgePart1 = BridgePart(self, 156)
    self.bridgePart2 = BridgePart(self, 155)
    
    self.boltCrank2 = BoltCrank(self, 56)
    self.bridgePart3 = BridgePart(self, 111)
    self.bridgePart4 = BridgePart(self, 116)
    
    self.boltCrank3 = BoltCrank(self, 160)
    self.bridgePart5 = PlatformBooster(self, 161, self.boltCrank3)
end
