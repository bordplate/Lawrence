function GetAPItemType(ap_item)
    if ap_item < 100 then
        return "item"
    elseif ap_item < 300 then
        return "planet"
    else
        return "gold bolt"
    end
end

function APItemToItem(ap_item) -- assumes user verified ap_item is an "item"
    return ap_item -- ap world is already lined up with real ids
end

function APItemToPlanet(ap_item) -- assumes user verified ap_item is an "planet"
    return ap_item - 100 -- ap world is already lined up with real ids +100
end

function APItemToGoldBolt(ap_item) -- assumes user verified ap_item is an "planet"
    -- translate gold bolts back to (planet, number)
    -- check if we will even care about this
    return ap_item
end