require 'ReplaceNPCMobys'
RandoUniverse = class("RandoUniverse", Universe)

function RandoUniverse:initialize(lobby)
    Universe.initialize(self)
    
    self.helga = self:GetLevelByName("Kerwan"):SpawnMoby(HelgaMoby)
    
    self.lobby = lobby
end