require 'RandoOptions'
require 'RandoUniverse'

Lobby = class("Lobby")

function Lobby:initialize(host, lobby_password)
    self.host = host
    self.password = lobby_password
    
    self.started = false
    
    self.archipelagoConnectingStatus = ""
    
    self.players = ObservableList({})

    self.options = RandoOptions({
        password = {
            name = "Lobby Password",
            description = "Password for the lobby. Leave blank for public.",
            handler = function(self, view, item) view.passwordInput:Activate() end,
            value = self.password,
        },
        address = {
            name = "Address",
            description = "The address for the archipelago server (usually 'archipelago.gg' or 'localhost')",
            handler = function(option, view, item) view.addressInput:Activate() end,
            value = "archipelago.gg",
            accessory = "archipelago.gg"
        },
        port = {
            name = "Port",
            description = "The archipelago port",
            handler = function(option, view, item) view.portInput:Activate() end,
        },
        slot = {
            name = "Slot",
            description = "The archipelago slot",
            handler = function(option, view, item) view.slotInput:Activate() end,
            value = "Player1",
            accessory = "Player1"
        },
        archipelagoPassword = {
            name = "Password",
            description = "The archipelago password (blank if none)",
            handler = function(option, view, item) view.archipelagoPasswordInput:Activate() end,
        },
        cheats = {
            name = "Cheats",
            description = "Enable cheats (currently only ghost ratchet)",
            handler = function(option, view, item) option:set(not option.value) end,
            value = false,
            accessory = {"On", "Off"}
        },
    })
    
    self.optionsList = { self.options.password, self.options.address, self.options.port, self.options.slot, self.options.archipelagoPassword, self.options.cheats }
    
    self.readyCallbacks = {}
    
    self.bolts = 0
    self.unlockedInfobots = {}
    self.unlockedSkillpoints = {}
    self.unlockedItems = {}
    
    self.address = "archipelago.gg"
    self.port = ""
    self.slot = "Player1"
    self.ap_password = ""
    
    self.connected = false
    self.waiting_on_connection = false
    self.startPlanet = 1
    self.universe = RandoUniverse(self)
end

function Lobby:AddReadyCallback(callback)
    table.insert(self.readyCallbacks, callback)
end

function Lobby:Start()
    --self.unlockedInfobots = {self.options.startPlanet.value}
    --
    --print("Starting lobby for: ")
    --for i, player in ipairs(self.players) do
    --    print("  " .. player:Username())
    --    
    --    player:CloseView()
    --    player:Start()
    --end
    --
    --self.started = true
    if not self.waiting_on_connection then
        self.waiting_on_connection = true
        self.archipelagoConnectingStatus = "Connecting to Archipelago"
        self.universe:Connect()
    end
end

function Lobby:ap_connected()
    self.connected = true
    self.universe.ap_client:GetBolts()
end

function Lobby:ap_refused()
    print("ap_refused")
    self.archipelagoConnectingStatus = "Connection Failed"
    self.waiting_on_connection = false
end

function Lobby:Join(player)
    player = player:Make(RandoPlayer)
    
    player.lobby = self
    self.players:Add(player)
    
    player:AddEntity(LobbyView(player, self))
end

function Lobby:Leave(player)
    self.players:Remove(player)
    if player:GUID() == self.host:GUID() then
        if #self.players > 0 then
            self.host = self.players[1]
            self.host:ToastMessage("You are now the host.", 300)
        else
            self.universe.ap_client_initialized = false
            if self.universe.ap_client ~= nil then
                if self.universe.ap_client.ap ~= nil then
                    self.universe.ap_client.ap = nil
                end
                self.universe.ap_client.running = false
                self.universe.ap_client = nil
            end
            collectgarbage("collect")
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
