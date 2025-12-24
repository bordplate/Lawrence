local dir_sep = package.config:sub(1, 1)
if GetOS() == "Windows" then
    package.cpath = package.cpath .. ';./mods/randomizer/?.dll'
elseif GetOS() == "Linux" then
    package.cpath = package.cpath .. ';./mods/randomizer/?.so'
else
    print("OS:" .. GetOS() .. " not supported")
end

local AP = require('lua-apclientpp')
assert(AP, 'Failed to load')

APClient = class("APClient")

function APClient:initialize(universe, game_name, items_handling, uuid, host, slot, password)
    self.running = true  -- set this to false to kill the coroutine
    self.first_socket_error = true
    self.game_name = game_name
    function on_socket_connected()
        print("Socket connected")
    end

    function on_socket_error(msg)
        print("Socket error: " .. msg)
        if self.first_socket_error then
            self.first_socket_error = false
        else
            self.ap = nil
            self.running = false
            collectgarbage("collect")
            universe.lobby:ap_refused()
        end
    end

    function on_socket_disconnected()
        print("Socket disconnected")
    end

    function on_room_info()
        print("Room info")
        self.ap:ConnectSlot(slot, password, items_handling, {"Lua-APClientPP"}, {0, 6, 1})
    end

    function on_slot_connected(slot_data)
        print("Slot connected")
        universe.slot_data = slot_data
        if slot_data["starting_planet"] ~= nil then
            starting_planet = slot_data["starting_planet"] - 100
            if starting_planet < 1 or starting_planet > 18 then
                print(string.format("starting planet %d is not a valid planet. defaulting to Novalis.", starting_planet))
                starting_planet = 1 -- novalis
            end
            universe.lobby.startPlanet = starting_planet
        end
        if slot_data["pack_size_gold_bolts"] ~= nil then
            universe.gold_bolt_pack_size = slot_data["pack_size_gold_bolts"]
        end
        if slot_data["pack_size_bolts"] ~= nil then
            universe.boltPackSize = slot_data["pack_size_bolts"]
        end
        if slot_data["metal_bolt_multiplier"] ~= nil then
            universe.metal_bolt_multiplier = slot_data["metal_bolt_multiplier"]
        end
        if slot_data["enable_bolt_multiplier"] ~= nil then
            universe.boltMultiplier = slot_data["enable_bolt_multiplier"]
        end

        if slot_data["progressive_weapons"] ~= nil then
            print("progressive weapons:" .. tostring(slot_data["progressive_weapons"]))
            universe.progressive_weapons = slot_data["progressive_weapons"]
        else
            universe.using_outdated_AP = true
        end
                
        for k,v in ipairs(slot_data) do
          print(string.format("%s: %d", k ,v))
        end
        universe.lobby:ap_connected()
        print("after ap_connectec call")
    end

    function on_slot_refused(reasons)
        print("Slot refused: " .. table.concat(reasons, ", "))
        self.ap = nil
        self.running = false
        collectgarbage("collect")
        universe.lobby:ap_refused()
    end

    function on_items_received(items)
       print("Archipelago Items received:")
       for _,v in ipairs(items) do
           universe:GiveAPItemToPlayers(v["item"], v["location"])
       end
    end

    function on_retrieved(map)
        if map["bolts"] ~= nil then
            print(string.format("got %d bolts from datastore", map["bolts"]))
            universe:GiveBolts(map["bolts"], false)
        end
        if map["completed_veldin_1"] ~= nil then
            print("veldin 1 was already completed, proceeding to starting planet")
            universe.completed_veldin_1 = true
        end
        universe.lobby:ap_retrieved_completed()
    end

    function on_print_json(msg, extra)
        local msg_str = self.ap:render_json(msg, AP.RenderFormat.TEXT)
        if extra ~= nil and (extra['type'] == "ItemSend" or extra['type'] == "ItemCheat") then
            universe:APMessageReceived(msg_str)
        end
    end
    


    print("before AP create. uuid: " .. tostring(uuid) .. " game_name: " .. tostring(game_name) .. " host: " .. tostring(host))
    self.ap = AP(uuid, game_name, host)
    self.ap:set_socket_connected_handler(on_socket_connected)
    self.ap:set_socket_error_handler(on_socket_error)
    self.ap:set_socket_disconnected_handler(on_socket_disconnected)
    self.ap:set_room_info_handler(on_room_info)
    self.ap:set_slot_connected_handler(on_slot_connected)
    self.ap:set_slot_refused_handler(on_slot_refused)
    self.ap:set_items_received_handler(on_items_received)
    self.ap:set_retrieved_handler(on_retrieved)
    --self.ap:set_print_json_handler(on_print_json)
end

function APClient:getLocation(location_id)
    self.ap:LocationChecks({location_id})
end

function APClient:SetBolts(totalBolts)
    self.ap:Set("bolts", 0, false, {{"replace", totalBolts}})
end

function APClient:Veldin1Completed()
    self.ap:Set("completed_veldin_1", false, false, {{"replace", true}})
end

function APClient:RetrieveDataStore()
    self.ap:Get({"bolts", "completed_veldin_1"})
end

function APClient:SendHint(location_id)
    self.ap:LocationScouts({location_id}, 2)
end

function APClient:WinGame()
    self.ap:StatusUpdate(30)
end

function APClient:poll()
    if self.running then
        self.ap:poll()
    end
end

