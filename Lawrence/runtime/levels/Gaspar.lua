require 'runtime.levels.Gaspar.MetalElevator'

Gaspar = class("Gaspar", Level)

function Gaspar:initialize(internalEntity)
    Level.initialize(self, internalEntity)

    self:LoadHybrids()
end

function Gaspar:LoadHybrids()
    
end
