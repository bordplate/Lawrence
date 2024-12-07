require 'LobbyView'

LobbyListView = class("LobbyListView", View)

function LobbyListView:initialize()
    View.initialize(self)

    self.lobbyTextElement = TextElement(250, 10, "Co-op Lobbies")
    self.lobbyListMenu = ListMenuElement(0, 30, 250, 330)
    self.textArea = TextAreaElement(260, 30, 200, 150)
    self.gamemodeTextArea = TextAreaElement(260, 195, 200, 165)
    
    self.createLobbyButtonText = TextElement(380, 380, "\x11 Create lobby")
    
    self.lobbyListMenu.ItemSelected = function(index) end

    self.lobbyListMenu.ItemActivated = function(index) end
    
    self:AddElement(self.lobbyTextElement)
    self:AddElement(self.textArea)
    self:AddElement(self.lobbyListMenu)
    self:AddElement(self.gamemodeTextArea)
    self:AddElement(self.createLobbyButtonText)
    
    self.passwordInputElement = InputElement()
    self.passwordInputElement.Prompt = "Enter password (blank for public)"
    self.passwordInputElement.InputCallback = function(player, input)
        local lobby = LobbyView(player, input)
        player:ShowView(lobby)
    end
    
    self:AddElement(self.passwordInputElement)

    self.textArea.Text = "Select a lobby."
end

function LobbyListView:OnPresent()
    self.lobbyListMenu:Focus()
end

function LobbyListView:OnTick()
    
end

function LobbyListView:OnControllerInputPressed(player, input)
    if IsButton(input, Gamepad.Circle) then
        self.passwordInputElement:Activate()
    end
end
