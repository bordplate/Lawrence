CoopUniverse = class("CoopUniverse", Universe)

function CoopUniverse:initialize(lobby)
    Universe.initialize(self)
    
    self.lobby = lobby
end