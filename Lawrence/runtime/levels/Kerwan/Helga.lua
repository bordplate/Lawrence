Helga = class("Helga", HybridMoby)

function Helga:initialize(level, uid)
    print("Initializing Helga")
    
    HybridMoby.initialize(self, level, uid)
end

function Helga:OnAttributeChange(player, offset, oldValue, newValue)
    
end
