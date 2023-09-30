StartRandoMoby = class("StartRandoMoby", Moby)

function StartRandoMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    -- Crate
    self:SetOClass(556)

    self.rotationSpeed = 0.01*math.pi
    self.amplitude = 0.01  -- the height of the bouncing
    self.frequency = 0.001  -- how fast the bouncing occurs
    self.bounceZ = self.z

    self.alpha = 1
    self.scale = 0.2

    self.rotY = 0.0
end

function StartRandoMoby:OnTick()
    self.rotZ = self.rotZ + self.rotationSpeed * Game:DeltaTime()  -- update rotation
    self.bounceZ = (self.amplitude * math.sin(self.frequency * Game:Time()))
    self.z = self.z + self.bounceZ  -- update z position
end

startCasualRandoMoby = class ("startCasualRandoMoby", StartRandoMoby)
startSpeedrunRandoMoby = class ("startSpeedrunRandoMoby", StartRandoMoby)