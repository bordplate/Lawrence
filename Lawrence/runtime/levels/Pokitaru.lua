require 'runtime.levels.Common.Button'
require 'runtime.levels.Common.ThrusterPackLock'
require 'runtime.levels.Pokitaru.Puffoid'
require 'runtime.levels.Pokitaru.Psyctopus'

Pokitaru = class("Pokitaru", Level)

function Pokitaru:initialize(internalEntity)
    Level.initialize(self, internalEntity)

    self.puffoids = {}
    
    self:LoadHybrids()
end

function Pokitaru:LoadHybrids()
    local puffoids = {
        -- First section
        162,
        163,
        164,
        165,
        195,
        199,
        200,
        
        -- Second section
        187,
        188,
        216,
        217,
        219,
        659,
        660,
        661,
        662,
        663,
        664,
        665,
        666,
        667,
        668,
        673,
        674,
        696,
        697,
        698,
        699,
        700,
        701,
        702,
        
        -- Third section
        130,
        131,
        132,
        133,
        134,
        135,
        136,
        137,
        138,
        139,
        140,
        141,
        142,
        143,  -- This one is inside a wall ffs
        
        144,
        145,
        146,
        147,
        148,
        149,
        150,
        151,
        152,
        153,
        154,
        155,
        156,
        157,
        
        -- Water
        159,
        160,
        161,
        167,
        168,
        169,
        170,
        171,
        172,
        173,
        174,
        175,
        176,
        177,
        178,
        180,
        181,
        182,
        183,
        184,
        201,
        202,
        203,
        210,
        211,
        214,
    }
    
    for i = 1, #puffoids do
        self.puffoids[i] = Puffoid(self, puffoids[i])
    end
    
    self.psyctopus1 = Psyctopus(self, 166)
    self.psyctopus2 = Psyctopus(self, 189)
    
    self.bridgeButton = Button(self, 185)
    
    self.thruserPackLock = ThrusterPackLock(self, 630)
end
