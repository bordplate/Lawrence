require 'runtime.levels.Common.Button'

Rilgar = class("Rilgar", Level)

function Rilgar:initialize(internalEntity)
    Level.initialize(self, internalEntity)

    self.buttons = {}
    
    self:LoadHybrids()
end

function Rilgar:LoadHybrids()
    local buttons = {
        25, 26, 130, 294, 407, 408, 206, 213, 212, 204, 248, 182, 202, 208, 215, 216, 247, 205, 209, 203, 409, 246, 129
    }
    
    for _, uid in pairs(buttons) do
        self.buttons[#self.buttons+1] = Button(self, uid)
    end
end
