-- Add the runtime libraries to the package path
package.path = package.path .. ';./runtime/lib/?.lua'

require 'middleclass'
require 'bit'
require 'ObservableList'

require 'Entity'
require 'Universe'
require 'Player'
require 'Label'
require 'Moby'
require 'Item'

require 'runtime.ui.View'
require 'runtime.ui.ViewElement'
require 'runtime.ui.TextAreaElement'
require 'runtime.ui.ListMenuElement'
require 'runtime.ui.TextElement'
require 'runtime.ui.InputElement'

require 'HybridMoby'

require 'Level'
require 'runtime.levels.Novalis'
require 'runtime.levels.Kerwan'
require 'runtime.levels.Eudora'
require 'runtime.levels.BlargStation'
require 'runtime.levels.Rilgar'
require 'runtime.levels.Umbris'
require 'runtime.levels.Gaspar'
require 'runtime.levels.Orxon'
require 'runtime.levels.Pokitaru'
require 'runtime.levels.DreksFleet'

null = {}

Gamepad = {
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

function IsButton(input, button)
    return input & button ~= 0
end

math.randomseed(os.time())

function millisToTime(millis)
    local total_seconds = math.floor(millis / 1000)
    local minutes = math.floor(total_seconds / 60)
    local seconds = total_seconds - (minutes * 60)
    local milliseconds = millis - (total_seconds * 1000)
    return string.format("%02d:%02d.%03d", minutes, seconds, milliseconds)
end

function millisToTimeSeconds(millis)
    local total_seconds = math.floor(millis / 1000)
    local minutes = math.floor(total_seconds / 60)
    local seconds = total_seconds - (minutes * 60)
    return string.format("%02d:%02d", minutes, seconds)
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

function distance_between_3d_points(a, b)
    return math.sqrt((a.x - b.x)^2 + (a.y - b.y)^2 + (a.z - b.z)^2)
end

function RGBA(r, g, b, a)
    return bit.blshift(a, 24) + bit.blshift(b, 16) + bit.blshift(g, 8) + r
end
