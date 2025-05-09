TestMoby = class("TestMoby", Moby)

function TestMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    -- Crate
    self:SetOClass(0)
    
    self.AnimationId = 22
    
    self.modeBits = 0

    self.rotationSpeed = 0.01*math.pi
    self.amplitude = 0.01  -- the height of the bouncing
    self.frequency = 0.001  -- how fast the bouncing occurs
    self.bounceZ = self.z

    self.alpha = 1
    self.scale = 0.2

    self.rotY = 0.0
end

function TestMoby:OnTick()
    self.rotZ = self.rotZ + self.rotationSpeed * Game:DeltaTime()  -- update rotation
    --self.bounceZ = (self.amplitude * math.sin(self.frequency * Game:Time()))
    --self.z = self.z + self.bounceZ  -- update z position
end