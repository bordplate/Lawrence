require "mobys.Novalis.Piston"
require "mobys.Novalis.PistonActivator"
require "mobys.Novalis.ElevatorTest"
require "mobys.Novalis.PistonSeesaw"
require "mobys.Novalis.DoorLeft"
require "mobys.Novalis.DoorRight"
require "mobys.Novalis.PipeButton"

require "mobys.Enemy"

CoopUniverse = class("CoopUniverse", Universe)

function CoopUniverse:initialize(lobby)
    Universe.initialize(self)
    
    self.lobby = lobby
end

function CoopUniverse:OnPlayerLeave(player)
    
end

function Universe:InitTrueCoop()
    local novalis = self:GetLevelByName("Novalis")
    
    -- First room with Pistons on way to Plumber
    self.piston1 = Piston(novalis, 90, 82.5, 85)
    self.piston1.active = true
    
    self.piston1Activator = novalis:SpawnMoby(PistonActivator)
    self.piston1Activator.pistons = {self.piston1}
    self.piston1Activator:SetPosition(252, 127, 81)
    self.piston1Activator2 = novalis:SpawnMoby(PistonActivator)
    self.piston1Activator2.pistons = {self.piston1}
    self.piston1Activator2:SetPosition(259.5, 150, 85)
    
    -- Second piston room with all the water and stuff
    self.piston2 = Piston(novalis, 92, 86.5, 90.5)
    self.piston2.active = false
    self.piston3 = Piston(novalis, 93, 90.5, 94.5)
    self.piston3.active = true

    self.pistonsActivator1 = novalis:SpawnMoby(PistonActivator)
    self.pistonsActivator1.pistons = {self.piston2, self.piston3}
    self.pistonsActivator1:SetPosition(277, 166.5, 85)
    self.pistonsActivator2 = novalis:SpawnMoby(PistonActivator)
    self.pistonsActivator2.pistons = {self.piston2, self.piston3}
    self.pistonsActivator2:SetPosition(294, 174, 95)
    
    self.elevator = novalis:SpawnMoby(ElevatorTest)
    self.elevator:SetPosition(157, 146, 60.5)
    
    self.pistonSeesaw1 = novalis:SpawnMoby(PistonSeesaw)
    self.pistonSeesaw1:SetPosition(64, 182, 46.5)
    
    self.pistonSeesaw2 = novalis:SpawnMoby(PistonSeesaw)
    self.pistonSeesaw2:SetPosition(64.8, 184.5, 45)
    
    self.pistonSeesaw3 = novalis:SpawnMoby(PistonSeesaw)
    self.pistonSeesaw3.scale = 0.45
    self.pistonSeesaw3:SetPosition(66.1, 187, 51)
    
    self.pistonSeesaw1.linkedPiston = self.pistonSeesaw2
    
    self.doorLeft = novalis:SpawnMoby(DoorLeft)
    self.doorLeft:SetPosition(73.2, 180.6, 43.75)
    self.doorLeft.rotZ = 63

    self.doorRight = novalis:SpawnMoby(DoorRight)
    self.doorRight:SetPosition(73.2, 180.6, 43.75)
    self.doorRight.rotZ = 63
    
    self.pipeButton = novalis:SpawnMoby(PipeButton)
    self.pipeButton.doorLeft = self.doorLeft
    self.pipeButton.doorRight = self.doorRight
    self.pipeButton.rotZ = -30
    self.pipeButton:SetPosition(78.1, 181.4, 44.0)
    
    self.doorLeft2 = novalis:SpawnMoby(DoorLeft)
    self.doorLeft2.scale = 0.3
    self.doorLeft2:SetPosition(70.8, 187.6, 45.5)
    self.doorLeft2.rotZ = 152

    self.doorRight2 = novalis:SpawnMoby(DoorRight)
    self.doorRight2.scale = 0.3
    self.doorRight2:SetPosition(70.8, 187.6, 45.5)
    self.doorRight2.rotZ = 152
    
    self.pipeButton2 = novalis:SpawnMoby(PipeButton)
    self.pipeButton2.doorLeft = self.doorLeft2
    self.pipeButton2.doorRight = self.doorRight2
    self.pipeButton2:SetPosition(66, 187.7, 51)
    
    local pokitaru = self:GetLevelByName("Pokitaru")
    
    self.testEnemy = pokitaru:SpawnMoby(Enemy)
    self.testEnemy:SetPosition(500, 418, 230)
end
