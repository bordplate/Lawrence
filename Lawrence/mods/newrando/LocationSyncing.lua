local locationToActionMap = {
    -- novalis
    [1] = function (universe, player) universe.replacedMobys:GetMoby('Plumber'):Disable() end, -- plumber
    [2] = function (universe, player) print("mayor trigger") end, -- mayor
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
            LocationSync(universe, _player, location_id)
        end
    end
end 