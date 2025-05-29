LobbyView = class("LobbyView", View)

function LobbyView:initialize(player, lobby)
    View.initialize(self, player)
    
    self.lobby = lobby
    
    self.passwordInput = InputElement()
    self.passwordInput.Prompt = "Enter password"
    self.passwordInput.InputCallback = function(input)
        if self.PlayerTable:GUID() == self.lobby.host:GUID() then
            self.lobby.password = input
            
            if input == "" then
                self.optionsListMenu:GetItem(0).Accessory = "Not set"
            else
                self.optionsListMenu:GetItem(0).Accessory = "Change"
            end
        end
    end

    self.addressInput = InputElement()
    self.addressInput.Prompt = "Enter Archipelago Address"
    self.addressInput.InputCallback = function(input)
        self.lobby.address = input
        self.optionsListMenu:GetItem(1).Accessory = input
    end
    
    self.portInput = InputElement()
    self.portInput.Prompt = "Enter Archipelago Port"
    self.portInput.InputCallback = function(input)
        self.lobby.port = input
        self.optionsListMenu:GetItem(2).Accessory = input
    end

    self.slotInput = InputElement()
    self.slotInput.Prompt = "Enter Archipelago Slot name"
    self.slotInput.InputCallback = function(input)
        self.lobby.slot = input
        self.optionsListMenu:GetItem(3).Accessory = input
    end

    self.archipelagoPasswordInput = InputElement()
    self.archipelagoPasswordInput.Prompt = "Enter Archipelago Password}"
    self.archipelagoPasswordInput.InputCallback = function(input)
        self.lobby.ap_pass = input
        if input == "" then
            self.optionsListMenu:GetItem(4).Accessory = "Not set"
        else
            self.optionsListMenu:GetItem(4).Accessory = "Change"
        end
    end
    
    self.lobbyTextElement = TextElement(40, 10, "Players")
    self.optionsTextElement = TextElement(250, 10, "Options")
    
    self.playersList = ListMenuElement(0, 30, 200, 330)
    for i, player in ipairs(self.lobby.players) do
        self:AddPlayerToList(player)
    end
    self.lobby.players:AddObserver(function(list, action, item)
        if action == ObservableList.ADDED then
            self:AddPlayerToList(item)
        end
        if action == ObservableList.REMOVED then
            print("Removing player " .. item:Username())
            
            for i, listItem in ipairs(self.playersList:GetItems()) do
                if listItem.Title == item:Username() then
                    self.playersList:RemoveItem(i-1)
                    break
                end
            end
        end
    end)
    
    self.optionsListMenu = ListMenuElement(210, 30, 250, 215)
    self.descriptionTextArea = TextAreaElement(210, 250, 250, 110)
    
    for i, option in ipairs(self.lobby.optionsList) do
        local accessory = ""
        
        if option.accessory ~= nil then
            if type(option.value) == "boolean" then
                accessory = option.accessory[option.value and 1 or 2]
            elseif type(option.value) == "number" then
                accessory = option.accessory[option.value+1]
            elseif type(option.value) == "string" then
                accessory = option.value
            end
        end
        
        self.optionsListMenu:AddItem(option.name, "", accessory)
    end
    
    self.lobby.options:AddObserver(function(list, key, value)
        for k, option in ipairs(self.lobby.optionsList) do
            if option.name == list[key].name then
                if type(value) == "boolean" then
                    self.optionsListMenu:GetItem(k-1).Accessory = value and option.accessory[1] or option.accessory[2]
                elseif type(value) == "number" then
                    self.optionsListMenu:GetItem(k-1).Accessory = option.accessory[value+1]
                elseif type(option.value) == "string" then
                    self.optionsListMenu:GetItem(k-1).Accessory = value
                end

                break
            end
        end
    end)
    
    self.optionsListMenu.ItemSelected = function(index)
        self.descriptionTextArea.Text = self.lobby.optionsList[index+1].description
    end
    
    self.optionsListMenu.ItemActivated = function(index)
        if (self.lobby.host:GUID() == self.PlayerTable:GUID()) then
            self.lobby.optionsList[index+1](self, self.optionsListMenu:GetItem(index))
        end
    end
    
    self.descriptionTextArea.Text = self.lobby.optionsList[1].description
    
    self.backButtonText = TextElement(120, 390, "\x12 Exit")
    self.startButtonText = TextElement(380, 390, "\x11 Start")

    if self.lobby.password == "" then
        self.optionsListMenu:GetItem(0).Accessory = "Not set"
    else
        self.optionsListMenu:GetItem(0).Accessory = "Change"
    end
    
    self.optionsListMenu:GetItem(1).Accessory = self.lobby.address
    self.optionsListMenu:GetItem(2).Accessory = self.lobby.port
    self.optionsListMenu:GetItem(3).Accessory = self.lobby.slot
    if self.lobby.ap_password == "" then
        self.optionsListMenu:GetItem(4).Accessory = "Not set"
    else
        self.optionsListMenu:GetItem(4).Accessory = "Change"
    end
    
    
    self.lobby:AddReadyCallback(function(player)
        for i, item in ipairs(self.playersList:GetItems()) do
            if item.Title == player:Username() then
                item.Accessory = player.ready and "Ready" or "Not Ready"
            end
        end
    end)
    
    self:AddElement(self.lobbyTextElement)
    self:AddElement(self.optionsTextElement)
    self:AddElement(self.playersList)
    self:AddElement(self.descriptionTextArea)
    self:AddElement(self.optionsListMenu)
    self:AddElement(self.passwordInput)
    self:AddElement(self.addressInput)
    self:AddElement(self.portInput)
    self:AddElement(self.slotInput)
    self:AddElement(self.archipelagoPasswordInput)
    
    self:AddElement(self.backButtonText)
    self:AddElement(self.startButtonText)
end

function LobbyView:OnPresent()
    if (self.lobby.host:GUID() == self.PlayerTable:GUID()) then
        self.startButtonText.Text = "\x11 Start"
    else
        self.startButtonText.Text = "\x11 Ready"
    end
    
    self.optionsListMenu:Focus()
end

function LobbyView:OnControllerInputPressed(input)
    if IsButton(input, Gamepad.Circle) then
        if self.lobby.started then
            self.PlayerTable:CloseView()
            self.PlayerTable:Start()
            return
        end
        
        if self.PlayerTable:GUID() ~= self.lobby.host:GUID() then
            self.lobby:PlayerReady(self.PlayerTable)

            if self.PlayerTable.ready then
                self.startButtonText.Text = "\x11 Unready"
            else
                self.startButtonText.Text = "\x11 Ready"
            end
        elseif self.lobby:AllPlayersReady() then
            self.lobby:Start()
        end
    end

    if IsButton(input, Gamepad.Triangle) then
        self.lobby:Leave(self.PlayerTable)
    end
end

function LobbyView:AddPlayerToList(player)
    local details = ""
    local accessory = player.ready and "Ready" or "Not Ready"

    if player:GUID() == self.PlayerTable:GUID() then
        details = "You"
    end

    if player:GUID() == self.lobby.host:GUID() then
        details = "Host"
        accessory = ""
    end
    
    if player.ingame then
        accessory = "In Game"
    end

    self.playersList:AddItem(player:Username(), details, accessory)
end

function LobbyView:OnTick()
    if self.PlayerTable:GUID() == self.lobby.host:GUID() then
        if not self.lobby:AllPlayersReady() then
            self.startButtonText.TextColor = RGBA(0xA0, 0xA0, 0xA0, 0xc0)
        else
            self.startButtonText.TextColor = RGBA(0x88, 0xa8, 0xff, 0xc0)
        end
    end
end
