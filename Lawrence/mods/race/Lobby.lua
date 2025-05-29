require 'RaceOptions'
require 'Race.RaceUniverse'

Lobby = class("Lobby")

function Lobby:initialize(host, password)
    self.host = host
    self.password = password
    
    self.started = false
    
    self.players = ObservableList({})

    self.options = RaceOptions({
        password = {
            name = "Password",
            description = "Password for the lobby. Leave blank for public.",
            handler = function(self, view, item) view.passwordInput:Activate() end,
            value = password,
        },
        startPlanet = {
            name = "Starting race",
            description = "Which race to start with",
            handler = function(option, view, item) option:set((option.value+1) % 19) end,
            value = 0,
            accessory = {
                "Orxon",
                "Aridia",
                "Eudora Backwards",
                "Gemlik",
                "Kalebo III",
                "Pokitaru",
                "Rilgar",
            }
        }
    })
    
    self.optionsList = { self.options.password }
    
    self.readyCallbacks = {}
    
    self.bolts = 0
    self.unlockedInfobots = {}
    self.unlockedSkillpoints = {}
    self.unlockedItems = {}
    
    self.universe = RaceUniverse(self)
end

function Lobby:AddReadyCallback(callback)
    table.insert(self.readyCallbacks, callback)
end

function Lobby:Start()
    print("Starting lobby for: ")
    for i, player in ipairs(self.players) do
        print("  " .. player:Username())
    end
    
    self.universe:StartWaiting()

    self.started = true
end

function Lobby:Join(player)
    
    player.lobby = self
    self.players:Add(player)
    self.universe:AddEntity(player)
    
    player:AddEntity(LobbyView(player, self))
end

function Lobby:Leave(player)
    self.players:Remove(player)
    
    if player:GUID() == self.host:GUID() then
        if #self.players > 0 then
            self.host = self.players[1]
            self.host:ToastMessage("You are now the host.", 300)
        else
            lobbyUniverse:RemoveLobby(self)
        end
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
