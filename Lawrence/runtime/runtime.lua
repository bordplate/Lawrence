-- Add the runtime libraries to the package path
package.path = package.path .. ';./runtime/lib/?.lua'

require 'middleclass'
require 'Entity'
require 'Universe'
require 'Player'
require 'Label'
require 'Moby'

math.randomseed(os.time())

function millisToTime(millis)
    local total_seconds = math.floor(millis / 1000)
    local minutes = math.floor(total_seconds / 60)
    local seconds = total_seconds - (minutes * 60)
    local milliseconds = millis - (total_seconds * 1000)
    return string.format("%02d:%02d.%03d", minutes, seconds, milliseconds)
end

function pickUniqueItems(tbl, num_items)
    if #tbl < num_items then
        return "Insufficient number of items in table."
    end

    local picked = {}
    local indices = {}

    for i = 1, num_items do
        local index
        repeat
            index = math.random(1, #tbl)
        until not indices[index]

        indices[index] = true
        table.insert(picked, tbl[index])
    end

    return picked
end
