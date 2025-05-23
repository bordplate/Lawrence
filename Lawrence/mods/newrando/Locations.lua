local itemLocations = {
    -- Novalis
    [16]=3, -- pyrocitor
    
    -- Aridia
    [30]=7, -- hoverboard
    [26]=8, -- tresspasser
    [5]=9, -- sonic summoner
    
    -- Kerwan
    [12]=14, -- swingshot
    [2]=15, -- heli pack
    [15]=17, -- blaster
    
    -- Eudora
    [9]=22, -- suck cannon
    [20]=23, -- glove of doom
    
    -- Rilgar
    [48]=26, -- zoomerator
    [17]=27, -- mine glove
    [23]=28, -- RYNO
    
    -- Blarg
    [22]=31, -- hydrodisplacer
    [29]=33, -- grindboots
    [14]=34, -- taunter
    
    -- Umbris
    
    -- Batalia
    [11]=40, -- devastator
    [27]=43, -- metal detector
    
    -- Gaspar
    [18]=46, -- walloper
    [7]=47, -- pilot helmet
    
    -- Orxon
    [13]=50, -- visibomb
    [28]=53, -- magneboots
    [52]=54, -- premium nanotech
    [53]=55, -- ultra nanotech
    
    -- Pokitaru
    [25]=58, -- decoy glove
    [6]=59, -- O2 mask
    [35]=60, -- persuader
    [3]=61, -- thruster pack
    
    -- Hoven
    [24]=63, -- drone device
    [4]=65, -- hydro pack
    [49]=66, -- raritanium
    
    -- Gemlik
    
    -- Oltanis
    [19]=71, -- tesla claw
    [32]=73, -- PDA
    [21]=74, -- morph o ray
    
    -- Quartu
    [34]=80, -- bolt grabber
    
    -- Kalebo III
    [31]=84, -- hologuise
    [33]=85, -- map o matic
    
    -- Drek's Fleet
    [50]=89, -- codebot
}

local planetLocations = {
    -- Novalis
    [2]=1, -- aridia
    [3]=2, -- kerwan
    
    -- Aridia
    
    -- Kerwan
    [4]=16, -- eudora
    
    -- Eudora
    [6]=21, -- blarg
    
    -- Rilgar
    [7]=25, -- umbris
    
    -- Blarg
    [5]=32, -- rilgar
    
    -- Umbris
    [8]=37, -- batalia
    
    -- Batalia
    [9]=41, -- gaspar
    [10]=42, -- orxon
    
    -- Gaspar
    
    -- Orxon
    [11]=51, -- pokitaru
    [12]=52, -- hoven
    
    -- Pokitaru
    
    -- Hoven
    [13]=64, -- gemlik
    
    -- Gemlik
    [14]=69, -- oltanis
    
    -- Oltanis
    [15]=72, -- quartu
    
    -- Quartu
    [16]=79, -- kalebo III
    [17]=81, -- Drek's Fleet
    
    -- Drek's Fleet
    [18]=88, -- veldin 2
}

local boltLocation = {
    -- Novalis
    ["1-0"]=4,
    ["1-2"]=5,
    ["1-1"]=6,
    
    -- Aridia
    ["2-0"]=13,
    ["2-1"]=11,
    ["2-2"]=12,
    ["2-3"]=10,
    
    -- Kerwan
    ["3-0"]=18,
    ["3-1"]=20,
    ["3-2"]=19,
    
    -- Eudora
    ["4-0"]=24,
    
    -- Rilgar
    ["5-0"]=29,
    ["5-1"]=30,
    
    -- Blarg
    ["6-0"]=35,
    ["6-1"]=36,
        
    -- Umbris
    ["7-0"]=39,
    ["7-1"]=38,
    
    -- Batalia
    ["8-0"]=44,
    ["8-1"]=45,
    
    -- Gaspar
    ["9-0"]=49,
    ["9-1"]=48,
    
    -- Orxon
    ["10-0"]=57,
    ["10-1"]=56,
    
    -- Pokitaru
    ["11-0"]=62,
    
    -- Hoven
    ["12-0"]=68,
    ["12-1"]=67,
    
    -- Gemlik
    ["13-0"]=70,
    
    -- Oltanis
    ["14-0"]=78,
    ["14-1"]=75,
    ["14-2"]=76,
    ["14-3"]=77,
    
    -- Quartu
    ["15-0"]=82,
    ["15-1"]=83,
    
    -- Kalebo III
    ["16-0"]=87,
    ["16-1"]=86,
    
    -- Drek's Fleet
    ["17-0"]=90,
    ["17-1"]=91,
    
    -- Veldin 2
    ["18-0"]=92,
    ["18-1"]=93,
    ["18-2"]=94,
}

local planetVendorItems = {
    [1] = 0x10,
    [3] = 0x0f,
    [4] = 0x14,
    [5] = 0x11,
    [6] = 0x0e,
    [8] = 0x0b,
    [9] = 0x12,
    [10] = 0x0d,
    [11] = 0x19,
    [12] = 0x18,
    [14] = 0x13,
}

function ItemToLocation(item_id)
    if itemLocations[item_id] ~= nil then
        return itemLocations[item_id]
    end
    print(string.format("item_id: %d not found", item_id))
    return 0
end

function PlanetToLocation(planet_id)
    print("planet_id = " .. tostring(planet_id))
    if planetLocations[planet_id] ~= nil then
        return planetLocations[planet_id]
    end
    print(string.format("planet_id: %d not found", planet_id))
    return 0
end

function GoldBoltToLocation(planet, number)
    bolt_id = string.format("%d-%d", planet, number)
    if boltLocation[bolt_id] ~= nil then
        return boltLocation[bolt_id]
    end
    print(string.format("bolt_id: %s not found", bolt_id))
    return 0
end

function GetPlanetVendorItem(planet_id) 
    return planetVendorItems[planet_id]
end
