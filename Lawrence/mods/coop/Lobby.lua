require 'CoopOptions'
require 'CoopUniverse'

Lobby = class("Lobby")

function Lobby:initialize(host, password)
    self.host = host
    self.password = password
    
    self.started = false
    
    self.players = ObservableList({})

    self.options = CoopOptions({
        password = {
            name = "Password",
            description = "Password for the lobby. Leave blank for public.",
            handler = function(self, view, item) view.passwordInput:Activate() end,
            value = password,
        },
        friendlyFire = {
            name = "Friendly fire",
            description = "When enabled lets players hurt each other with weapons and the wrench.",
            handler = function(option, view, item) option:set(not option.value) end,
            value = true,
            accessory = {"On", "Off"}
        },
    })
    
    self.optionsList = { self.options.password, self.options.friendlyFire }
    
    self.readyCallbacks = {}
    
    self.universe = CoopUniverse(self)
end

function Lobby:AddReadyCallback(callback)
    table.insert(self.readyCallbacks, callback)
end

function Lobby:Start()
    self.started = true
    
    for i, player in ipairs(self.players) do
        player:CloseView()
        player:Start()
    end
end

function Lobby:Join(player)
    player = player:Make(CoopPlayer)
    
    player.lobby = self
    self.players:Add(player)
    self.universe:AddEntity(player)
    
    player:ShowView(LobbyView(player, self))
end

function Lobby:Leave(player)
    self.players:Remove(player)
    
    if player:GUID() == self.host:GUID() then
        for i, player in ipairs(self.players) do
            self:Leave(player)
        end
        
        lobbyUniverse:RemoveLobby(self)
    end

    lobbyUniverse:AddEntity(player)
end

function Lobby:PlayerReady(player)
    player.ready = not player.ready
    
    for i, callback in ipairs(self.readyCallbacks) do
        callback(player)
    end
end

function Lobby:AllPlayersReady()
    for i, player in ipairs(self.players) do
        if not player.ready and self.host:GUID() ~= player:GUID() then
            return false
        end
    end
    
    return true
end
