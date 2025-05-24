local locationToActionMap = {
    -- novalis
    [1] = function (universe, player) universe.replacedMobys:GetMoby('Plumber'):Disable() end, -- Plumber
    [2] = function (universe, player) player:SetLevelFlags(1,1,0,{0xff}) end, -- Mayor
    [3] = function (universe, player) universe:RemoveVendorItem(0x10) end, -- pyrocitor
    [4] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 1 * 4 + 0, 1, 1) end, -- sewer gold bolt
    [5] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 1 * 4 + 2, 1, 1) end, -- caves gold bolt
    [6] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 1 * 4 + 1, 1, 1) end, -- underwater caves gold bolt
    
    -- Aridia
     [7] = function (universe, player) universe.replacedMobys:GetMoby('Skid'):Disable() end, -- Skid
     [8] = function (universe, player) universe.replacedMobys:GetMoby('Trespasser'):Disable() end, -- trespasser
     [9] = function (universe, player) universe.replacedMobys:GetMoby('Agent'):Disable() end, -- Skids agent
     [10] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 2 * 4 + 3, 1, 1) end, -- tresspasser gold bolt
     [11] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 2 * 4 + 1, 1, 1) end, -- island gold bolt
     [12] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 2 * 4 + 2, 1, 1) end, -- magneboots gold bolt
     [13] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 2 * 4 + 0, 1, 1) end, -- sandshark gold bolt
    
    -- Kerwan
    [14] = function (universe, player) universe.replacedMobys:GetMoby('Helga'):Disable() end, -- Helga
    [15] = function (universe, player) universe.replacedMobys:GetMoby('Al'):Disable() player:SetLevelFlags(2, 3, 78, {1}) end, -- Al
    [16] = function (universe, player) universe.replacedMobys:GetMoby('KerwanInfobot'):Disable() end, -- Train infobot
    [17] = function (universe, player) universe:RemoveVendorItem(0x0f) end, -- blaster
    [18] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 3 * 4 + 0, 1, 1) end, -- belows ship gold bolt
    [19] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 3 * 4 + 2, 1, 1) end, -- train station gold botl
    [20] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 3 * 4 + 1, 1, 1) end, -- lone tower gold bolt
    
    -- Eudora
    [21] = function (universe, player) player:SetLevelFlags(1, 4, 1, {0xff}) player:SetLevelFlags(2, 4, 16, {128}) player:SetLevelFlags(2, 4, 41, {16}) end, -- Henchman
    [22] = function (universe, player) universe.replacedMobys:GetMoby('SuckCannon'):Disable() end, -- suck cannon
    [23] = function (universe, player) universe:RemoveVendorItem(0x14) end, -- glove of doom
    [24] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 4 * 4 + 0, 1, 1) end, -- gold bolt
    
    -- Rilgar
    [25] = function (universe, player) universe.replacedMobys:GetMoby('Bouncer'):Disable() end, -- Bouncer
    [26] = function (universe, player) universe.replacedMobys:GetMoby('Zoomerator'):Disable() end, -- zoomerator
    [27] = function (universe, player) universe:RemoveVendorItem(0x11) end, -- mine glove
    [28] = function (universe, player) universe.replacedMobys:GetMoby('Salesman'):Disable() end, -- ryno
    [29] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 5 * 4 + 0, 1, 1) end, -- maze gold bolt
    [30] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 5 * 4 + 1, 1, 1) end, -- waterworks gold bolt
    
    -- Blarg
    [31] = function (universe, player) universe.replacedMobys:GetMoby('Hydrodisplacer'):Disable() end, -- Hydrodisplacer
    [32] = function (universe, player) player:SetLevelFlags(1, 6, 0, {0xff}) end, -- Explosion Infobot
    [33] = function (universe, player) universe.replacedMobys:GetMoby('Scientist'):Disable() end, -- Scientist
    [34] = function (universe, player) universe:RemoveVendorItem(0x0e) end, -- taunter
    [35] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 6 * 4 + 0, 1, 1) end, -- outside gold bolt
    [36] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 6 * 4 + 1, 1, 1) end, -- swarmer gold bolt
    
    -- Umbris
    [37] = function (universe, player) universe.replacedMobys:GetMoby('UmbrisInfobot'):Disable() end, -- infobot
    [38] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 7 * 4 + 1, 1, 1) end, -- puzzle gold bolt
    [39] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 7 * 4 + 0, 1, 1) end, -- jump down gold bolt
    
    -- Batalia
    [40] = function (universe, player) universe:RemoveVendorItem(0x0b) end, -- devestator
    [41] = function (universe, player) universe.replacedMobys:GetMoby('Deserter'):Disable() end, -- deserter
    [42] = function (universe, player) universe.replacedMobys:GetMoby('Commando'):Disable() end, -- commando
    [43] = function (universe, player) player:SetLevelFlags(1, 8, 4, {0xff}) if player:Level():GetName() == "Batalia" then player:DeleteAllChildrenWithUID(271) end end, -- metal detector
    [44] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 8 * 4 + 0, 1, 1) end, -- cliffside gold bolt
    [45] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 8 * 4 + 1, 1, 1) end, -- tresspasser gold bolt
    
    -- Gaspar
    [46] = function (universe, player) universe:RemoveVendorItem(0x12) end, -- walloper
    [47] = function (universe, player) universe.replacedMobys:GetMoby('PilotHelmet'):Disable() end, -- pilot helmet
    [48] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 9 * 4 + 1, 1, 1) end, -- swingshot gold bolt
    [49] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 9 * 4 + 0, 1, 1) end, -- volcano gold bolt
    
    -- Orxon
    [50] = function (universe, player) universe:RemoveVendorItem(0x0d) end, -- visibomb
    [51] = function (universe, player) universe.replacedMobys:GetMoby('OrxonClankInfobot'):Disable() player:SetLevelFlags(2, 10, 30, {112}) player:SetLevelFlags(2, 10, 103, {1}) if player:Level():GetName() == "Orxon" then player:DeleteAllChildrenWithUID(255) end end, -- clank infobot
    [52] = function (universe, player) universe.replacedMobys:GetMoby('OrxonRatchetInfobot'):Disable() player:SetLevelFlags(1, 10, 4, {0xff}) end, -- ratchet infobot
    [53] = function (universe, player) universe.replacedMobys:GetMoby('Magneboots'):Disable() player:SetLevelFlags(1, 10, 1, {0xff}) player:SetLevelFlags(2, 10, 35, {2}) end, -- magneboots
    [54] = function (universe, player) universe.replacedMobys:GetMoby('NanotechVendor'):Progress() player:SetLevelFlags(2, 10, 31, {1}) player:SetLevelFlags(2, 10, 87, {2}) player:SetLevelFlags(1, 10, 0, {0xff}) end, -- premium nanotech
    [55] = function (universe, player) universe.replacedMobys:GetMoby('NanotechVendor'):Disable() end, -- ultra nanotech
    [56] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 10 * 4 + 0, 1, 1) end, -- clank gold bolt
    [57] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 10 * 4 + 1, 1, 1) end, -- visibomb gold bolt
    
    -- Pokitaru
    [58] = function (universe, player) universe:RemoveVendorItem(0x19) end, -- decoy glove
    [59] = function (universe, player)  end, -- O2 mask. do nothing
    [60] = function (universe, player) universe.replacedMobys:GetMoby('Fred'):Disable() player:SetLevelFlags(1, 11, 4, {0xff}) end, -- persuader
    [61] = function (universe, player) universe.replacedMobys:GetMoby('Bob'):Disable() end, -- thruster pack
    [62] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 11 * 4 + 0, 1, 1) end, -- gold bolt
    
    -- Hoven
    [63] = function (universe, player) universe:RemoveVendorItem(0x18) end, -- drone device
    [64] = function (universe, player) end, -- infobot. do nothing
    [65] = function (universe, player) universe.replacedMobys:GetMoby('Edwina'):Disable() end, -- hydro pack
    [66] = function (universe, player) universe.replacedMobys:GetMoby('Miner'):Disable() end, -- raritanium
    [67] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 12 * 4 + 1, 1, 1) end, -- water gold bolt
    [68] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 12 * 4 + 0, 1, 1) end, -- walljump gold bolt
    
    -- Gemlik
    [69] = function (universe, player) print("probably level flags?") end, -- qwark infobot
    [70] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 13 * 4 + 0, 1, 1) end, -- gold bolt
    
    -- Oltanis
    [71] = function (universe, player) universe:RemoveVendorItem(0x13) end, -- tesla claw
    [72] = function (universe, player) universe.replacedMobys:GetMoby('Sam'):Disable() player:SetLevelFlags(1, 14, 1, {0xff}) player:SetLevelFlags(1, 14, 7, {0xff}) player:SetLevelFlags(2, 14, 0, {2}) end, -- infobot
    [73] = function (universe, player) universe.replacedMobys:GetMoby('Steve'):Disable() player:SetLevelFlags(1, 14, 0, {0xff}) player:SetLevelFlags(1, 14, 11, {0xff}) player:SetLevelFlags(2, 14, 1, {16}) player:SetLevelFlags(2, 14, 46, {1}) player:SetLevelFlags(2, 14, 47, {8}) end, -- Steve
    [74] = function (universe, player) universe.replacedMobys:GetMoby('MorphORay'):Disable() player:SetLevelFlags(1, 14, 2, {0xff}) player:SetLevelFlags(1, 14, 8, {0xff}) end, -- morph O ray
    [75] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 14 * 4 + 1, 1, 1) end, -- main gold bolt
    [76] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 14 * 4 + 2, 1, 1) end, -- magnet gold bolt 2
    [77] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 14 * 4 + 3, 1, 1) end, -- magnet gold bolt 2
    [78] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 14 * 4 + 0, 1, 1) end, -- final gold bolt
    
    -- Quartu
    [79] = function (universe, player) end, -- giant clank infobot. do nothing
    [80] = function (universe, player) universe.replacedMobys:GetMoby('BoltGrabber'):Disable() end, -- bolt grabber
    [81] = function (universe, player) end, -- infiltrate infobot. do nothing
    [82] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 15 * 4 + 0, 1, 1) end, -- mom gold bolt
    [83] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 15 * 4 + 1, 1, 1) end, -- codebot gold bolt
    
    -- Kalebo III
    [84] = function (universe, player) universe.replacedMobys:GetMoby('Hologuise'):Disable() end, -- hologuise. do nothing
    [85] = function (universe, player)  end, -- helpdesk lady. do nothing
    [86] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 16 * 4 + 1, 1, 1) end, -- grindrail gold bolt
    [87] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 16 * 4 + 0, 1, 1) end, -- break room gold bolt

    -- Drek's Fleet
    [88] = function (universe, player) universe.replacedMobys:GetMoby('FleetInfobot'):Disable() player:SetLevelFlags(1, 17, 3, {0xff}) end, -- infobot
    [89] = function (universe, player) universe.replacedMobys:GetMoby('Codebot'):Disable() end, -- codebot
    [90] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 17 * 4 + 0, 1, 1) end, -- water gold bolt
    [91] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 17 * 4 + 1, 1, 1) end, -- robot gold bolt
    
    -- Veldin
    [92] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 18 * 4 + 0, 1, 1) end, -- taunter gold bolt
    [93] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 18 * 4 + 1, 1, 1) end, -- halfway gold bolt
    [94] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 18 * 4 + 2, 1, 1) end, -- grind gold bolt
}

function LocationSync(universe, player, location_id)
    if locationToActionMap[location_id] ~= nil then
        locationToActionMap[location_id](universe, player)
    else
        print("missed table for location: " .. tostring(location_id))
    end
end

function PlayerCollectedLocation(universe, player, location_id)
    for _, _player in ipairs(universe:LuaEntity():FindChildren("Player")) do
        if _player ~= player then
            -- if not in list of level flag setters
            LocationSync(universe, _player, location_id)
        end
    end
end 

function PlayerResync(universe, player, location_id_list)
    for _, location_id in ipairs(location_id_list) do
        LocationSync(universe, player, location_id)
    end
end