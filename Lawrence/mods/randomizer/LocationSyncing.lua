local locationToActionMap = {
    -- novalis
    [1] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_level_array + 2, 1, 1) end, -- Plumber
    [2] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_level_array + 3, 1, 1) player:SetLevelFlags(1,1,0,{0xff}) universe.got_novalis_mayor = true end, -- Mayor
    [3] = function (universe, player) universe:RemoveVendorItem(0x10) end, -- pyrocitor
    [4] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 1 * 4 + 0, 1, 1) end, -- sewer gold bolt
    [5] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 1 * 4 + 2, 1, 1) end, -- caves gold bolt
    [6] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 1 * 4 + 1, 1, 1) end, -- underwater caves gold bolt
    [95] = function (universe, player) universe.replacedMobys:GetMoby('BombGloveCase'):Disable(universe) end,
    [96] = function (universe, player) universe.replacedMobys:GetMoby('PyrocitorCase'):Disable(universe) end,
    [97] = function (universe, player) universe.replacedMobys:GetMoby('BlasterCase'):Disable(universe) end,
    [98] = function (universe, player) universe.replacedMobys:GetMoby('GloveOfDoomCase'):Disable(universe) end,
    [99] = function (universe, player) universe.replacedMobys:GetMoby('SuckCannonCase'):Disable(universe) end,
    [100] = function (universe, player) universe.replacedMobys:GetMoby('TeslaClawCase'):Disable(universe) end,
    [101] = function (universe, player) universe.replacedMobys:GetMoby('DevastatorCase'):Disable(universe) end,
    [102] = function (universe, player) universe.replacedMobys:GetMoby('MineGloveCase'):Disable(universe) end,
    [103] = function (universe, player) universe.replacedMobys:GetMoby('MorphORayCase'):Disable(universe) end,
    [104] = function (universe, player) universe.replacedMobys:GetMoby('DecoyGloveCase'):Disable(universe) end,

    -- Aridia
     [7] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x1e, 1, 1) end, -- Skid
     [8] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x1a, 1, 1) end, -- trespasser
     [9] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x5, 1, 1) end, -- Skids agent
     [10] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 2 * 4 + 3, 1, 1) end, -- tresspasser gold bolt
     [11] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 2 * 4 + 1, 1, 1) end, -- island gold bolt
     [12] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 2 * 4 + 2, 1, 1) end, -- magneboots gold bolt
     [13] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 2 * 4 + 0, 1, 1) end, -- sandshark gold bolt
    
    -- Kerwan
    [14] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0xc, 1, 1) end, -- Helga
    [15] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x2, 1, 1) player:SetLevelFlags(2, 3, 78, {1}) end, -- Al
    [16] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_level_array + 4, 1, 1) end, -- Train infobot
    [17] = function (universe, player) universe:RemoveVendorItem(0x0f) end, -- blaster
    [18] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 3 * 4 + 0, 1, 1) end, -- belows ship gold bolt
    [19] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 3 * 4 + 2, 1, 1) end, -- train station gold botl
    [20] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 3 * 4 + 1, 1, 1) end, -- lone tower gold bolt
    
    -- Eudora
    [21] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_level_array + 6, 1, 1) player:SetLevelFlags(1, 4, 1, {0xff}) player:SetLevelFlags(2, 4, 16, {128}) player:SetLevelFlags(2, 4, 41, {16}) end, -- Henchman
    [22] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x9, 1, 1) end, -- suck cannon
    [23] = function (universe, player) universe:RemoveVendorItem(0x14) end, -- glove of doom
    [24] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 4 * 4 + 0, 1, 1) end, -- gold bolt
    
    -- Rilgar
    [25] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_level_array + 7, 1, 1) end, -- Bouncer
    [26] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x30, 1, 1) end, -- zoomerator
    [27] = function (universe, player) universe:RemoveVendorItem(0x11) end, -- mine glove
    [28] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x17, 1, 1) end, -- ryno
    [29] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 5 * 4 + 0, 1, 1) end, -- maze gold bolt
    [30] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 5 * 4 + 1, 1, 1) end, -- waterworks gold bolt
    
    -- Blarg
    [31] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x16, 1, 1) player:SetLevelFlags(1, 6, 1, {0xff}) end, -- Hydrodisplacer
    [32] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_level_array + 5, 1, 1) player:SetLevelFlags(1, 6, 0, {0xff}) end, -- Explosion Infobot
    [33] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x1d, 1, 1) end, -- Scientist
    [34] = function (universe, player) universe:RemoveVendorItem(0x0e) end, -- taunter
    [35] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 6 * 4 + 0, 1, 1) end, -- outside gold bolt
    [36] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 6 * 4 + 1, 1, 1) end, -- swarmer gold bolt
    
    -- Umbris
    [37] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_level_array + 8, 1, 1) end, -- infobot
    [38] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 7 * 4 + 1, 1, 1) end, -- puzzle gold bolt
    [39] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 7 * 4 + 0, 1, 1) end, -- jump down gold bolt
    
    -- Batalia
    [40] = function (universe, player) universe:RemoveVendorItem(0x0b) end, -- devestator
    [41] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_level_array + 9, 1, 1) end, -- deserter
    [42] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_level_array + 10, 1, 1) end, -- commando
    [43] = function (universe, player) player:SetLevelFlags(1, 8, 4, {0xff}) if player:Level():GetName() == "Batalia" then player:DeleteAllChildrenWithUID(271) end end, -- metal detector
    [44] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 8 * 4 + 0, 1, 1) end, -- cliffside gold bolt
    [45] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 8 * 4 + 1, 1, 1) end, -- tresspasser gold bolt
    
    -- Gaspar
    [46] = function (universe, player) universe:RemoveVendorItem(0x12) end, -- walloper
    [47] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x7, 1, 1) print("did the thing") end, -- pilot helmet
    [48] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 9 * 4 + 1, 1, 1) end, -- swingshot gold bolt
    [49] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 9 * 4 + 0, 1, 1) end, -- volcano gold bolt
    
    -- Orxon
    [50] = function (universe, player) universe:RemoveVendorItem(0x0d) end, -- visibomb
    [51] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_level_array + 11, 1, 1) player:SetLevelFlags(2, 10, 30, {112}) player:SetLevelFlags(2, 10, 103, {1}) end, -- clank infobot
    [52] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_level_array + 12, 1, 1) player:SetLevelFlags(1, 10, 4, {0xff}) player:SetLevelFlags(2, 87, 4, {2}) player:SetLevelFlags(2, 10, 102, {48}) end, -- ratchet infobot
    [53] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x1c, 1, 1) player:SetLevelFlags(1, 10, 1, {0xff}) player:SetLevelFlags(2, 10, 35, {2}) player:SetLevelFlags(1, 10, 1, {0xff}) end, -- magneboots
    [54] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x34, 1, 1) player:SetLevelFlags(2, 10, 31, {1}) player:SetLevelFlags(2, 10, 87, {2}) player:SetLevelFlags(1, 10, 0, {0xff}) end, -- premium nanotech
    [55] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x35, 1, 1) end, -- ultra nanotech
    [56] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 10 * 4 + 1, 1, 1) end, -- clank gold bolt
    [57] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 10 * 4 + 0, 1, 1) end, -- visibomb gold bolt
    
    -- Pokitaru
    [58] = function (universe, player) universe:RemoveVendorItem(0x19) end, -- decoy glove
    [59] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x6, 1, 1) end, -- O2 mask
    [60] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x23, 1, 1) player:SetLevelFlags(1, 11, 4, {0xff}) end, -- persuader
    [61] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x3, 1, 1) end, -- thruster pack
    [62] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 11 * 4 + 0, 1, 1) end, -- gold bolt
    
    -- Hoven
    [63] = function (universe, player) universe:RemoveVendorItem(0x18) end, -- drone device
    [64] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_level_array + 13, 1, 1) end, -- infobot. do nothing
    [65] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x4, 1, 1) end, -- hydro pack
    [66] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x31, 1, 1) end, -- raritanium
    [67] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 12 * 4 + 1, 1, 1) end, -- water gold bolt
    [68] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 12 * 4 + 0, 1, 1) end, -- walljump gold bolt
    
    -- Gemlik
    [69] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_level_array + 14, 1, 1) player:SetLevelFlags(2,13,113,{3}) end, -- qwark infobot
    [70] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 13 * 4 + 0, 1, 1) end, -- gold bolt
    
    -- Oltanis
    [71] = function (universe, player) universe:RemoveVendorItem(0x13) end, -- tesla claw
    [72] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_level_array + 15, 1, 1) universe.got_oltanis_infobot = true player:SetLevelFlags(1, 14, 1, {0xff}) player:SetLevelFlags(1, 14, 7, {0xff}) player:SetLevelFlags(2, 14, 0, {2}) FixPlanetsForPlayer(universe, player) end, -- infobot
    [73] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x20, 1, 1) universe.got_oltanis_PDA = true player:SetLevelFlags(1, 14, 0, {0xff}) player:SetLevelFlags(1, 14, 11, {0xff}) player:SetLevelFlags(2, 14, 1, {16}) player:SetLevelFlags(2, 14, 46, {1}) player:SetLevelFlags(2, 14, 47, {8}) FixPlanetsForPlayer(universe, player) end, -- Steve
    [74] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x15, 1, 1) universe.got_oltanis_morph = true player:SetLevelFlags(1, 14, 2, {0xff}) player:SetLevelFlags(1, 14, 8, {0xff}) FixPlanetsForPlayer(universe, player) end, -- morph O ray
    [75] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 14 * 4 + 1, 1, 1) end, -- main gold bolt
    [76] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 14 * 4 + 2, 1, 1) end, -- magnet gold bolt 2
    [77] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 14 * 4 + 3, 1, 1) end, -- magnet gold bolt 2
    [78] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 14 * 4 + 0, 1, 1) end, -- final gold bolt
    
    -- Quartu
    [79] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_level_array + 16, 1, 1) end, -- giant clank infobot. do nothing
    [80] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x22, 1, 1) end, -- bolt grabber
    [81] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_level_array + 17, 1, 1) end, -- infiltrate infobot. do nothing
    [82] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 15 * 4 + 0, 1, 1) end, -- mom gold bolt
    [83] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 15 * 4 + 1, 1, 1) end, -- codebot gold bolt
    
    -- Kalebo III
    [84] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x1f, 1, 1) end, -- hologuise. do nothing
    [85] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x21, 1, 1) end, -- helpdesk lady. do nothing
    [86] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 16 * 4 + 1, 1, 1) end, -- grindrail gold bolt
    [87] = function (universe, player) player:SetAddressValue(Player.offset.goldBolts + 16 * 4 + 0, 1, 1) end, -- break room gold bolt

    -- Drek's Fleet
    [88] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_level_array + 18, 1, 1) player:SetLevelFlags(1, 17, 3, {0xff}) end, -- infobot
    [89] = function (universe, player) player:SetAddressValue(Player.offset.bullshit_item_array + 0x32, 1, 1) end, -- codebot
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

function FixPlanetsForPlayer(universe, player)
    levelName = player:Level():GetName()
    if levelName == "Orxon" then
        if universe.has_o2_mask then
            player:SetLevelFlags(2, 10, 30, {112}) player:SetLevelFlags(2, 10, 103, {1})
            player:SetLevelFlags(1, 10, 1, {0xff}) player:SetLevelFlags(2, 10, 35, {2}) player:SetLevelFlags(1, 10, 1, {0xff})
            player:SetLevelFlags(1, 10, 4, {0xff}) player:SetLevelFlags(2, 87, 4, {2}) player:SetLevelFlags(2, 10, 102, {48}) -- ratchet got infobot flag
            player:DeleteAllChildrenWithUID(83) -- last gadgebot door (not gate)
            player:DeleteAllChildrenWithUID(673) -- main gate
            player:DeleteAllChildrenWithUID(812) -- minor gate (right)
            player:DeleteAllChildrenWithUID(813) -- minor gate (left)
        end
    elseif levelName == "BlargStation" then
        if universe.has_o2_mask then
            player:SetLevelFlags(1, 6, 1, {0xff})
        end
    elseif levelName == "Oltanis" then
        if universe.got_oltanis_infobot and universe.got_oltanis_PDA and universe.got_oltanis_morph then
            player:DeleteAllChildrenWithUID(370) -- statue
        end
    end
end