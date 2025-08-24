function GetAPItemType(ap_item)
    if ap_item == 48 or ap_item == 49 or ap_item == 50 or ap_item == 52 or ap_item == 53 then
        return "special"
    elseif ap_item < 80 then
        return "item"
    elseif ap_item < 100 then
        return "progressive"
    elseif ap_item < 200 then
        return "planet"
    elseif ap_item < 300 then
        return "skill point"
    elseif ap_item == 301 then
        return "gold bolt"
    elseif ap_item == 302 then
        return "bolt pack"
    elseif ap_item < 500 then
        return "special"
    else
        return "gold bolt"
    end
end

function APItemToItem(ap_item) -- assumes user verified ap_item is an "item"
    return ap_item -- ap world is already lined up with real ids
end

function APItemToSpecial(ap_item) -- assumes user verified ap_item is an "item"
    if ap_item == 48 then
        return Player.offset.has_zoomerator
    elseif ap_item == 49 then
        return Player.offset.has_raritanium
    elseif ap_item == 50 then
        return Player.offset.has_codebot
    elseif ap_item == 52 then
        return Player.offset.has_premium_nanotech
    elseif ap_item == 53 then
        return Player.offset.has_ultra_nanotech
    elseif ap_item >= 309 and ap_item <= 325 then
        return Player.offset.gildedItems + ap_item - 300
    else
        return ap_item -- should not happen
    end
end

function APItemToPlanet(ap_item) -- assumes user verified ap_item is an "planet"
    return ap_item - 100 -- ap world is already lined up with real ids +100
end

function APItemToGoldWeapon(ap_item) -- assumes user verified ap_item is an "gold weapon"
    return ap_item - 300
end

function APItemToGoldBolt(ap_item) -- assumes user verified ap_item is an "planet"
    -- translate gold bolts back to (planet, number)
    -- check if we will even care about this
    return ap_item
end

function ProgressiveAPItemToNormalAPItem(ap_item, slot_data, unlock_count)
    if unlock_count[ap_item] == nil then
        unlock_count[ap_item] = 0
    end
    unlock_count[ap_item] = unlock_count[ap_item] + 1
    
    if ap_item == 80 then -- Progressive Pack
        return slot_data["progressive_packs_order"][unlock_count[ap_item]]
    elseif ap_item == 81 then -- Progressive Helmet
        return slot_data["progressive_helmets_order"][unlock_count[ap_item]]
    elseif ap_item == 82 then -- Progressive Suck Cannon
        return slot_data["progressive_suck_cannon_order"][unlock_count[ap_item]]
    elseif ap_item == 83 then -- Progressive Bomb glove
        return slot_data["progressive_bomb_glove_order"][unlock_count[ap_item]]
    elseif ap_item == 84 then -- Progressive Devastator
        return slot_data["progressive_devastator_order"][unlock_count[ap_item]]
    elseif ap_item == 85 then -- Progressive Blaster
        return slot_data["progressive_blaster_order"][unlock_count[ap_item]]
    elseif ap_item == 86 then -- Progressive Pyrocitor
        return slot_data["progressive_pyrocitor_order"][unlock_count[ap_item]]
    elseif ap_item == 87 then -- Progressive Mine glove
        return slot_data["progressive_mine_glove_order"][unlock_count[ap_item]]
    elseif ap_item == 88 then -- Progressive Tesla claw
        return slot_data["progressive_tesla_claw_order"][unlock_count[ap_item]]
    elseif ap_item == 89 then -- Progressive Glove of doom
        return slot_data["progressive_glove_of_doom_order"][unlock_count[ap_item]]
    elseif ap_item == 90 then -- Progressive Morph-o-ray
        return slot_data["progressive_morph_o_ray_order"][unlock_count[ap_item]]
    elseif ap_item == 91 then -- Progressive Decoy glove
        return slot_data["progressive_decoy_glove_order"][unlock_count[ap_item]]
    elseif ap_item == 92 then -- Progressive Boots
        return slot_data["progressive_boots_order"][unlock_count[ap_item]]
    elseif ap_item == 93 then -- Progressive Hoverboard
        return slot_data["progressive_hoverboard_order"][unlock_count[ap_item]]
    elseif ap_item == 94 then -- Progressive Raritanium
        return slot_data["progressive_raritanium_order"][unlock_count[ap_item]]
    elseif ap_item == 95 then -- Progressive Nanotech
        return slot_data["progressive_nanotech_order"][unlock_count[ap_item]]
    else
        print("progressive item: " .. tostring(ap_item) .. " not found...")
        return 0
    end
end

function APGoldWeaponToAPBaseWeapon(ap_item)
    if ap_item == 309 then -- Golden Suck Cannon
        return 9
    elseif ap_item == 310 then -- Golden Bomb glove
        return 10
    elseif ap_item == 311 then -- Golden Devastator
        return 11
    elseif ap_item == 315 then -- Golden Blaster
        return 15
    elseif ap_item == 316 then -- Golden Pyrocitor
        return 16
    elseif ap_item == 317 then -- Golden Mine glove
        return 17
    elseif ap_item == 319 then -- Golden Tesla claw
        return 19
    elseif ap_item == 320 then -- Golden Glove of doom
        return 20
    elseif ap_item == 321 then -- Golden Morph-o-ray
        return 21
    elseif ap_item == 325 then -- Golden Decoy glove
        return 25
    else
        print("gold weapon: " .. tostring(ap_item) .. " not found...")
        return 0
    end
end