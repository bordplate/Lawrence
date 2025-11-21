CoopUniverse = class("CoopUniverse", Universe)

function CoopUniverse:initialize(lobby)
    Universe.initialize(self)
    
    self.lobby = lobby
end

function CoopUniverse:OnPlayerLeave(player)
    print("Player " .. player:Username() .. " left, " .. #self:FindChildren("Player") .. " remaining.")

    if #self:FindChildren("Player") <= 0 then
        print("No more players, deleting universe.")
        self:Delete()
    end
end