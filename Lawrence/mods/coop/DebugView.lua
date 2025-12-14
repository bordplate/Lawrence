DebugView = class("DebugView", View)

function DebugView:initialize(player)
    self.player = player
    View.initialize(self)
    
    self.debugMenuOpen = false
    self.subMenuOpen = false
    self.showCoords = true
    self.itemsMenuPage = 1

    self.giveBoltsInput = InputElement()
    self.giveBoltsInput.Prompt = "Enter amount of bolts"
    self.giveBoltsInput.InputCallback = function(input)
        bolts = tonumber(input)
        if bolts == nil then
            self.player:ToastMessage(input .. " is not a number", 60)
            return
        end
        
        self.player:GiveBolts(bolts)
    end


    self.coordsTextArea = TextAreaElement(0, 350, 220, 60)
    self.mainMenu = ListMenuElement(0, 30, 250, 310)
    self.levelsMenu = ListMenuElement(260, 10, 250, 390)
    self.itemsMenu = ListMenuElement(260, 30, 250, 330)
    
    self.levelNames = {
        "Novalis",
        "Aridia",
        "Kerwan",
        "Eudora",
        "Rilgar",
        "Umbris",
        "BlargStation",
        "Batalia",
        "Gaspar",
        "Orxon",
        "Pokitaru",
        "Hoven",
        "GemlikStation",
        "Oltanis",
        "Quartu",
        "KaleboIII",
        "DreksFleet",
        "Veldin2",
    }

    for _, level in ipairs(self.levelNames) do
        self.levelsMenu:AddItem(level)
    end
    
    self.levelsMenu.ItemActivated = function(index)
        self.player:UnlockLevel(index+1)
        self.player:ToastMessage("Unlocked level " .. self.levelNames[index+1], 60)
    end
    
    self.levelsMenu.Visible = false
    
    self.items = {
        {
            Item.GetByName("Heli-pack"),
            Item.GetByName("Thruster-pack"),
            Item.GetByName("Hydro-pack"),
            Item.GetByName("Sonic Summoner"),
            Item.GetByName("O2 Mask"),
            Item.GetByName("Pilot's Helmet"),
            Item.GetByName("Swingshot"),
            Item.GetByName("Hydrodisplacer"),
            Item.GetByName("Trespasser"),
            Item.GetByName("Metal Detector"),
            Item.GetByName("Hologuise"),
            Item.GetByName("PDA"),
            Item.GetByName("Magneboots"),
            Item.GetByName("Grindboots"),
            Item.GetByName("Devastator"),
        }, {
            Item.GetByName("Visibomb"),
            Item.GetByName("Taunter"),
            Item.GetByName("Blaster"),
            Item.GetByName("Pyrociter"),
            Item.GetByName("Mine Glove"),
            Item.GetByName("Walloper"),
            Item.GetByName("Tesla Claw"),
            Item.GetByName("Glove of Doom"),
            Item.GetByName("Drone Device"),
            Item.GetByName("Decoy Glove"),
            Item.GetByName("Bomb Glove"),
            Item.GetByName("Suck Cannon"),
            Item.GetByName("Morph-o-Ray"),
            Item.GetByName("R.Y.N.O."),
            Item.GetByName("Hoverboard"),
        }
    }
    
    self:SetItemsPage(1)
    
    self.itemsMenu.ItemActivated = function(index)
        if self.itemsMenuPage == 1 and index == 15 then
            self:SetItemsPage(2)
            return
        elseif self.itemsMenuPage == 2 and index == 15 then
            self:SetItemsPage(1)
            return
        end
        
        self.player:GiveItem(self.items[self.itemsMenuPage][index+1].id)
    end
    
    self.itemsMenu.Visible = false
    
    self.mainMenuItems = {
        {
            name = "Levels",
            callback = function(item)
                self.levelsMenu.Visible = true
                self.levelsMenu:Focus()
                self.subMenuOpen = true
            end
        },
        {
            name = "Items",
            callback = function(item)
                self.itemsMenu.Visible = true
                self.itemsMenu:Focus()
                self.subMenuOpen = true
            end
        },
        {
            name = "Give bolts",
            callback = function(item)
                self.giveBoltsInput:Activate()
            end
        },
        {
            name = "Teleport to ship",
            callback = function(item)
                self:TeleportToShip()
            end
        },
        {
            name = "Die",
            callback = function(item)
                self:CloseDebugMenu()
                self.player:SetPosition(0, 0, -10000)
            end
        },
        {
            name = "Unlock all gold bolts",
            callback = function(item)
                self.player:UnlockAllGoldBolts()
            end
        },
        {
            name = "Show coords",
            accessory = "On",
            callback = function(item)
                self.showCoords = not self.showCoords
                item.Accessory = self.showCoords and "On" or "Off"
                self.coordsTextArea.Visible = self.showCoords
            end
        },
    }

    for _, item in ipairs(self.mainMenuItems) do
        local accessory = ""
        if item.accessory ~= nil then
            accessory = item.accessory
        end
        
        self.mainMenu:AddItem(item.name, "", accessory)
    end
    
    self.mainMenu.ItemActivated = function(index)
        if self.mainMenu.Visible then
            self.mainMenuItems[index+1].callback(self.mainMenu:GetItem(index))
        end
    end
    
    self.mainMenu.Visible = false
    
    self:AddElement(self.coordsTextArea)
    self:AddElement(self.mainMenu)
    self:AddElement(self.levelsMenu)
    self:AddElement(self.itemsMenu)
    self:AddElement(self.giveBoltsInput)
end

function DebugView:OnPresent()
    
end

function DebugView:OnControllerInputPressed(input)
    --print("Button! " .. input)
    
    if not self.debugMenuOpen and input & Gamepad.L3 ~= 0 then
        self.player.state = 114
        self.mainMenu.Visible = true
        self.mainMenu:Focus()
        self.debugMenuOpen = true
    elseif self.debugMenuOpen and IsButton(input, Gamepad.L3) then
        self.mainMenu:Focus()
        self.mainMenu.Visible = false
        self.levelsMenu.Visible = false
        self.itemsMenu.Visible = false
        
        self.player.state = 0
        self.debugMenuOpen = false
    end

    if self.debugMenuOpen and IsButton(input, Gamepad.Triangle) and self.subMenuOpen then
        self:CloseDebugMenu()
    end
end

function DebugView:CloseDebugMenu()
    self.levelsMenu.Visible = false
    self.itemsMenu.Visible = false
    self.mainMenu:Focus()

    self.subMenuOpen = false
end

function DebugView:OnTick()
    if self.showCoords then
        self.coordsTextArea.Text = "X: " .. self.player.x .. "\1Y: " .. self.player.y .. "\1Z: " .. self.player.z
    end 
end

function DebugView:TeleportToShip()
    local level = self.player:Level():GetName()

    if level == "Veldin1" then
        self.player:SetPosition(132.09, 115.480, 31.430)
    elseif level == "Novalis" then
        self.player:SetPosition(162.530, 136.393, 60.5)
    elseif level == "Aridia" then
        self.player:SetPosition(210.41, 170.35, 25.35)
    elseif level == "Kerwan" then
        self.player:SetPosition(263.982, 102.092, 54.5)
    elseif level == "Eudora" then
        self.player:SetPosition(220.250, 162.04, 56)
    elseif level == "Rilgar" then
        self.player:SetPosition(338.32, 110.8, 62.7)
    elseif level == "BlargStation" then
        self.player:SetPosition(247.950, 148.68, 138.3)
    elseif level == "Umbris" then
        self.player:SetPosition(264.55, 72.13, 45.77)
    elseif level == "Batalia" then
        self.player:SetPosition(151.52, 196.72, 37.83)
    elseif level == "Gaspar" then
        self.player:SetPosition(291.3, 392.3, 36.25)
    elseif level == "Orxon" then
        self.player:SetPosition(229.65, 203.13, 49.22)
    elseif level == "Pokitaru" then
        self.player:SetPosition(498.82, 406, 230)
    elseif level == "Hoven" then
        self.player:SetPosition(304.1, 303.24, 31.83)
    elseif level == "GemlikStation" then
        self.player:SetPosition(508.551, 391.111, 315.02)
    elseif level == "Oltanis" then
        self.player:SetPosition(255.79, 155.22, 47)
    elseif level == "Quartu" then
        self.player:SetPosition(308.6, 190, 32)
    elseif level == "KaleboIII" then
        self.player:SetPosition(146.43, 113.23, 128.3)
    elseif level == "DreksFleet" then
        self.player:SetPosition(500.833, 609.732, 152.5)
    elseif level == "Veldin2" then
        self.player:SetPosition(341.5, 632.7, 87.250)
    end
end

function DebugView:SetItemsPage(page)
    self.itemsMenu:ClearItems()
    self.itemsMenuPage = page
    
    for _, item in ipairs(self.items[page]) do
        self.itemsMenu:AddItem(item.name)
    end

    if page == 1 then
        self.itemsMenu:AddItem("Next...")
    else
        self.itemsMenu:AddItem("Previous...")
    end
end
