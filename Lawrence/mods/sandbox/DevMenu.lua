DevMenu = class("DevMenu", Player)

--[[
    Author: https://github.com/xFusionLordx
    License: Free to use, nothing else.
]]--

MENU = {
    display = "Sandbox Menu",
    value = {
        {
            display = "Planet Selector",
            value = {
                { display = "Veldin 1", value = "Veldin1" },
                { display = "Novalis", value = "Novalis" },
                { display = "Aridia", value = "Aridia" },
                { display = "Kerwan", value = "Kerwan" },
                { display = "Eudora", value = "Eudora" },
                { display = "Rilgar", value = "Rilgar" },
                { display = "Blarg Station", value = "BlargStation" },
                { display = "Umbris", value = "Umbris" },
                { display = "Batalia", value = "Batalia" },
                { display = "Gaspar", value = "Gaspar" },
                { display = "Orxon", value = "Orxon" },
                { display = "Pokitaru", value = "Pokitaru" },
                { display = "Hoven", value = "Hoven" },
                { display = "Gemliik Station", value = "GemlikStation" },
                { display = "Olantis", value = "Oltanis" },
                { display = "Quartu", value = "Quartu" },
                { display = "Kalebo III", value = "KaleboIII" },
                { display = "Drek's Fleet", value = "DreksFleet" },
                { display = "Veldin 2", value = "Veldin2" },
            },
            FUNCTION = function(player, planet)
                player:Close();
                player:LoadLevel(planet)
            end
        },
        {
            display = "Clank's Equipment",
            value = {
                { display = "Heli Pack", value = 0x02 },
                { display = "Thruster Pack", value = 0x03 },
                { display = "Hydro Pack", value = 0x04 },
            },
            FUNCTION = function(player, pack)
                player:GiveItem(pack, false)
            end
        },
        {
            display = "Helmets",
            value = {
                { display = "Sonic Summoner", value = 0x05 },
                { display = "O2 Mask", value = 0x06 },
                { display = "Pilots Helmet", value = 0x07 },
            },
            FUNCTION = function(player, helmet)
                player:GiveItem(helmet, false)
            end
        },
        {
            display = "Gadgets",
            value = {
                { display = "Swingshot", value = 0x0c },
                { display = "Hydrodisplacer", value = 0x16 },
                { display = "Trespasser", value = 0x1a },
                { display = "Metal_Detector", value = 0x1b },
                { display = "Hologuise", value = 0x1f },
                { display = "PDA", value = 0x20 },
            },
            FUNCTION = function(player, gadget)
                player:GiveItem(gadget, false)
            end
        },
        {
            display = "Boots",
            value = {
                { display = "Magneboots", value = 0x1c },
                { display = "Hrindboots", value = 0x1d },
            },
            FUNCTION = function(player, boots)
                player:GiveItem(boots, false)
            end
        },
        {
            display = "Items",
            value = {
                { display = "Hoverboard", value = 0x1e },
                { display = "Map-o-matic", value = 0x21 },
                { display = "Bolt Grabber", value = 0x22 },
                { display = "Persuader", value = 0x23 },
                { display = "Zoomerator", value = 0x30 },
                { display = "Raritanium", value = 0x31 },
                { display = "Codebot", value = 0x32 },
                { display = "Premium Nanotech", value = 0x34 },
                { display = "Ultra Nanotech", value = 0x35 },
            },
            FUNCTION = function(player, weapon)
                player:GiveItem(weapon, false)
            end
        },
        {
            display = "Weapons",
            value = {
                { display = "Devastator", value = 0x0b },
                { display = "Visibomb", value = 0x0d },
                { display = "Taunter", value = 0x0e },
                { display = "Blaster", value = 0x0f },
                { display = "Pyrociter", value = 0x10 },
                { display = "Mine Glove", value = 0x11 },
                { display = "Walloper", value = 0x12 },
                { display = "Tesla Claw", value = 0x13 },
                { display = "Glove of Doom", value = 0x14 },
                { display = "Drone Device", value = 0x18 },
                { display = "Decoy Glove", value = 0x19 },
                { display = "Bomb Glove", value = 0x0a },
                { display = "Suck Cannon", value = 0x09 },
                { display = "Morph-o-Ray", value = 0x15 },
                { display = "R.Y.N.O", value = 0x17 },
            },
            controls = "\x11 Golden",
            FUNCTION = function(player, weapon)
                player:GiveItem(weapon, false)
            end
        },
        {
            display = "Info Bots",
            value = {
                { display = "Novalis", value = 1 },
                { display = "Aridia", value = 2 },
                { display = "Kerwan", value = 3 },
                { display = "Eudora", value = 4 },
                { display = "Rilgar", value = 5 },
                { display = "Blarg Station", value = 6 },
                { display = "Umbris", value = 7 },
                { display = "Batalia", value = 8 },
                { display = "Gaspar", value = 9 },
                { display = "Orxon", value = 10 },
                { display = "Pokitaru", value = 0xb },
                { display = "Hoven", value = 0xc },
                { display = "Gemlik Station", value = 0xd },
                { display = "Oltanis", value = 0xe },
                { display = "Quartu", value = 0xf },
                { display = "Kalebo III", value = 0x10 },
                { display = "Drek's Fleet", value = 0x11 },
                { display = "Veldin", value = 0x12 },
            },
            FUNCTION = function(player, value)
                self:UnlockLevel(value)
            end
        },
        {
            display = "Bolts",
            value = {
                {
                    display = "Set Bolts",
                    value = {
                        { display = "0", value = 0 },
                        { display = "100", value = 100 },
                        { display = "1000", value = 1000 },
                        { display = "10000", value = 10000 },
                        { display = "100000", value = 100000 },
                        { display = "1000000", value = 1000000 },
                        { display = "MAX", value = 999999999 },
                    },
                    FUNCTION = function(player, bolts)
                        player:SetBolts(bolts)
                    end
                }
            }
        },
        {
            display = "Other Stuff",
            value = {
                {
                    display = "Unlock Goodies Menu",
                    value = {
                        addr = 0x969CD3,
                        value = 1

                    }
                }
            },
            FUNCTION = function(player, action) 
                player:WriteAddressValue(action.addr, action.value) 
            end
        }
    }
}

CONTROLLER_BUTTON = {
    L2 = 1,
    R2 = 2,
    L1 = 4,
    R1 = 8,
    Triangle = 16,
    Circle = 32,
    Cross = 64,
    Square = 128,
    Select = 256,
    L3 = 512,
    R3 = 1024,
    Start = 2048,
    Up = 4096,
    Right = 8192,
    Down = 16384,
    Left = 32768
}

function DevMenu:Made()
    self.menu = {
        enabled = false,
        category = MENU,
        page = 0,
        index = 1,
        pageSize = 13,
        x = 100,
        ySpace = 20
    }
    
    self.labels = {
        title = Label:new("", self.menu.x, 390 - (self.menu.ySpace * self.menu.pageSize) - self.menu.ySpace, 0xFF0FF000),
        controls = Label:new("", self.menu.x, 390, 0xC0FFA888),
    }
    
    for i = 1, self.menu.pageSize, 1 do
        local y = self.labels.title:Y();
        self.labels["item" .. i] = Label:new("", self.menu.x, y + (self.menu.ySpace * i), 0xC0FFA888)
    end
    
    for _, v in pairs(self.labels) do
        self:AddLabel(v)
    end
end

function DevMenu:RemoveBolts(amount)

end

function DevMenu:AddBolts(amount)
    
end

function DevMenu:OnControllerInputTapped(input)
    if (input == CONTROLLER_BUTTON.L3) then
        if (self.menu.enabled) then
            self:Close()
        else
            self.menu.enabled = true
            
            self:Update();
            self:ToastMessage("Dev menu enabled.", 60)
        end
    end
    
    if (self.menu.enabled) then
        if (input == CONTROLLER_BUTTON.Up) then
            self.menu.index = self.menu.index - 1
            
            if (self.menu.index < 1) then
                if (self.menu.page > 0) then
                    self.menu.page = self.menu.page - 1
                    self.menu.index = self.menu.pageSize
                else
                    self.menu.index = 1;
                end
            end
            
            self:Update();
        end
        
        if (input == CONTROLLER_BUTTON.Down) then
            self.menu.index = self.menu.index + 1
            
            if (self.menu.category.value[(self.menu.page * self.menu.pageSize) + self.menu.index] == nil) then
                self.menu.index = self.menu.index - 1
            end
            
            if (self.menu.index > self.menu.pageSize) then
                if (#self.menu.category.value > (self.menu.page * self.menu.pageSize)) then
                    self.menu.page = self.menu.page + 1
                    self.menu.index = 1
                else
                    self.menu.index = self.menu.pageSize
                end
            end
            
            self:Update();
        end
        
        if (input == CONTROLLER_BUTTON.Cross) then
            if (self.menu.category.FUNCTION == nil) then
                self.menu.category = self.menu.category.value[(self.menu.page * self.menu.pageSize) + self.menu.index]
                self.menu.index = 1;
            else
                self.menu.category.FUNCTION(self, self.menu.category.value[(self.menu.page * self.menu.pageSize) + self.menu.index].value)
            end
            
            self:Update();
        end
        
        if (input == CONTROLLER_BUTTON.Triangle) then
            -- Close menu if we are on the main menu
            if self.menu.category.display == "Sandbox Menu" then
                self:Close()
                return
            end
            
            if self.menu.category.value ~= MENU then
                self.menu.category = MENU
                self.menu.page = 0
                self.menu.index = 1
            end
            
            self:Update();
        end
    end
end

function DevMenu:Close()
    self:Clear();
    
    self.menu.enabled = false
    self.menu.category = MENU
    self.menu.page = 0
    self.menu.index = 1
    
    self:ToastMessage("Dev menu disabled.", 60)
    self:UnlockMovement();
    
    return true
end

function DevMenu:Clear()
    for _, v in pairs(self.labels) do
        v:SetText("")
    end
end

function DevMenu:Update()
    self.labels.title:SetText(self.menu.category.display)
    
    for i = 1, self.menu.pageSize, 1 do
        local s = self.menu.category.value[(self.menu.page * self.menu.pageSize) + i]
        
        if (s == nil) then s = "" else s = s.display end
        
        self.labels["item" .. i]:SetText(s)
        
        if (i == self.menu.index) then
            self.labels["item" .. i]:SetColor(0xC0FFA8FF)
        else
            self.labels["item" .. i]:SetColor(0xC0FFA888)
        end
    end
    
    local controls = ""
    
    if (self.menu.category.controls ~= nil) then
        controls = self.menu.category.controls
    end
    
    self.labels.controls:SetText("\x10: Select " .. controls .. "    \x12: Back")
end

function DevMenu:OnTick()
    if (self.menu.enabled) then
        self:LockMovement();
    end
end
