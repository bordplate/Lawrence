Moby = class('Moby', Entity)

function Moby:initialize(mobyEntity)
    Entity.initialize(self, mobyEntity)
end

function Moby:OnTick()
    Entity.OnTick(self)
end 