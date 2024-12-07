LobbyView = class("LobbyView", View)

function LobbyView:initialize(host, password)
    View.initialize(self)
    
    self.host = host
    self.password = password
    
    self.options = {
        {
            name = "Password",
            description = "Password for the lobby. Leave blank for public.",
            handler = function(item) self.passwordInput:Activate() end,
            default_on = null,
            default_accessory_text = self.password == "" and "Not set" or "Set"
        },
        {
            name = "Friendly fire", 
            description = "When enabled lets players hurt each other with weapons and the wrench.",
            handler = function(item) end,
            default_on = true,
        },
        {
            name = "Infinite ammo",
            description = "When enabled gives players infinite ammo for all weapons.",
            handler = function(item) end,
            default_on = false,
        }
    }
    
    self.passwordInput = InputElement()
    self.passwordInput.Prompt = "Enter password"
    self.passwordInput.InputCallback = function(player, input)
        if player:GUID() == self.host:GUID() then
            self.password = input
            
            if input == "" then
                self.optionsListMenu:GetItem(0).Accessory = "Not set"
            else
                self.optionsListMenu:GetItem(0).Accessory = "Set"
            end
        end
    end

    self.lobbyTextElement = TextElement(30, 10, "Players")
    self.optionsTextElement = TextElement(250, 10, "Options")
    
    self.playersList = ListMenuElement(0, 30, 200, 330)
    self.playersList:AddItem(self.host:Username(), "Host")
    
    self.optionsListMenu = ListMenuElement(210, 30, 250, 215)
    self.descriptionTextArea = TextAreaElement(210, 250, 250, 110)
    
    for i, option in ipairs(self.options) do
        local accessory_text = ""
        if option.default_on ~= null then
            accessory_text = option.default_on and "On" or "Off"
        end
        
        if option.default_accessory_text ~= nil then
            accessory_text = option.default_accessory_text
        end
        
        self.optionsListMenu:AddItem(option.name, "", accessory_text)
    end
    
    self.optionsListMenu.ItemSelected = function(index)
        self.descriptionTextArea.Text = self.options[index+1].description
    end
    
    self.optionsListMenu.ItemActivated = function(index)
        self.options[index+1].handler(self.optionsListMenu:GetItem(index))
    end

    self.descriptionTextArea.Text = self.options[1].description
    
    self.backButtonText = TextElement(120, 390, "\x12 Exit")
    self.startButtonText = TextElement(380, 390, "\x11 Start")
    
    self:AddElement(self.lobbyTextElement)
    self:AddElement(self.optionsTextElement)
    self:AddElement(self.playersList)
    self:AddElement(self.descriptionTextArea)
    self:AddElement(self.optionsListMenu)
    self:AddElement(self.passwordInput)
    
    self:AddElement(self.backButtonText)
    self:AddElement(self.startButtonText)
end

function LobbyView:OnPresent()
    self.optionsListMenu:Focus()
end

function LobbyView:OnControllerInputPressed(player, input)
    if IsButton(input, Gamepad.Circle) then
        player:LoadLevel("Kerwan")
    end

    if IsButton(input, Gamepad.Triangle) then
        player:ShowView(LobbyListView())
    end
end
