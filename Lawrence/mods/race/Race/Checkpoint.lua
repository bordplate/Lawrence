Checkpoint = class("Checkpoint", Moby)

function Checkpoint:initialize(internalEntity)
    Moby.initialize(self, internalEntity)
    
    -- Crate
    self:SetOClass(500)
    self.collision = false

    self.rotationSpeed = 0.01*math.pi
    self.amplitude = 0.01  -- the height of the bouncing
    self.frequency = 0.001  -- how fast the bouncing occurs
    self.bounceZ = self.z
    
    self.scale = 0.2
    self.alpha = 0.5
    
    self.rotY = 0.0
end

function Checkpoint:OnTick()
    self.rotZ = self.rotZ + self.rotationSpeed * Game:DeltaTime()  -- update rotation
    self.bounceZ = (self.amplitude * math.sin(self.frequency * Game:Time()))
    self.z = self.z + self.bounceZ  -- update z position

    if self:Parent() == nil and not self:Parent():LuaEntity():Is(RacePlayer) then
        print("Parent not player")
        return
    end

    if self:DistanceTo(self:Parent()) < 7.0 then
        self:Parent():LuaEntity():OnCollision(self)
    end
end