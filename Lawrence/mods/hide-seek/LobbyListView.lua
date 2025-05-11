require 'Lobby'
require 'LobbyView'

LobbyListView = class("LobbyListView", View)

function LobbyListView:initialize(player, lobbyUniverse)
    View.initialize(self, player)
    
    self.lobbyUniverse = lobbyUniverse

    self.lobbyTextElement = TextElement(250, 10, "Hide & Seek Lobbies")
    
    self.lobbyListMenu = ListMenuElement(0, 30, 250, 330)
    self.lobbyListMenu.ItemActivated = function(index)
        local lobby = self.lobbyUniverse.lobbies[index+1]
        if lobby == nil then
            return
        end
        
        if lobby.password == "" then
            lobby:Join(self.PlayerTable)
        else
            self.selectedLobby = lobby
            self.lobbyPasswordInputElement:Activate()
        end
    end
    self.lobbyListMenu.ItemSelected = function(index)
        local lobby = self.lobbyUniverse.lobbies[index+1]
        self:SelectedLobby(lobby)
    end
    
    for i, lobby in ipairs(self.lobbyUniverse.lobbies) do
        self:AddToLobbyList(lobby)
    end
    
    self.textArea = TextAreaElement(260, 30, 200, 150)
    self.textArea.Text = "Select a lobby."
    
    self.gamemodeTextArea = TextAreaElement(260, 195, 200, 165)
    
    self.createLobbyButtonText = TextElement(380, 380, "\x11 Create lobby")
    
    self.passwordInputElement = InputElement()
    self.passwordInputElement.Prompt = "Enter password (blank for public)"
    self.passwordInputElement.InputCallback = function(input)
        self.lobbyUniverse:NewLobby(self.PlayerTable, input)
    end
    
    self.selectedLobby = null
    
    self.lobbyPasswordInputElement = InputElement()
    self.lobbyPasswordInputElement.Prompt = "Enter lobby password"
    self.lobbyPasswordInputElement.InputCallback = function(input)
        if self.selectedLobby ~= null then
            if self.selectedLobby.password == input then
                self.selectedLobby:Join(self.PlayerTable)
            else
                self.PlayerTable:ShowErrorMessage("Incorrect password.")
            end
        end
    end
    
    self.lobbyUniverse.lobbies:AddObserver(function(list, action, item) 
        if action == ObservableList.ADDED then
            self:AddToLobbyList(item)
        end
        if action == ObservableList.REMOVED then
            for i, listItem in ipairs(self.lobbyListMenu:GetItems()) do
                if listItem.Title == item.host:Username() then
                    self.lobbyListMenu:RemoveItem(i-1)
                    break
                end
            end
        end
    end)

    self:AddElement(self.lobbyTextElement)
    self:AddElement(self.textArea)
    self:AddElement(self.lobbyListMenu)
    self:AddElement(self.gamemodeTextArea)
    self:AddElement(self.createLobbyButtonText)
    self:AddElement(self.passwordInputElement)
    self:AddElement(self.lobbyPasswordInputElement)
end

function LobbyListView:AddToLobbyList(lobby)
    local accessory = (lobby.started and "In game" or "In lobby") .. " - " ..
            (lobby.password == "" and "Open" or "Password protected")
    
    self.lobbyListMenu:AddItem(lobby.host:Username(), accessory)
end

function LobbyListView:OnPresent()
    self.lobbyListMenu:Focus()
end

function LobbyListView:OnTick()
    
end

function LobbyListView:SelectedLobby(lobby)
    self.textArea.Text = "Host: " .. lobby.host:Username()
    self.gamemodeTextArea.Text = ""
end

function LobbyListView:OnControllerInputPressed(input)
    if IsButton(input, Gamepad.Circle) then
        self.passwordInputElement:Activate()
    end
end
