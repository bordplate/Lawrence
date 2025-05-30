local dir_sep = package.config:sub(1, 1)
if dir_sep == '\\' then
    package.cpath = package.cpath .. ';./mods/newrando/?.dll'
else
    package.cpath = package.cpath .. ';./mods/newrando/?.so'
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
        for k,v in ipairs(slot_data) do
          print(string.format("%s: %d", k ,v))
        end
        universe.lobby:ap_connected()
        print("after ap_connectec call")
    end

    function on_slot_refused(reasons)
        print("Slot refused: " .. table.concat(reasons, ", ")) 
        universe.lobby:ap_refused()
    end

    function on_items_received(items)
       print("Archipelago Items received:")
       for _,v in ipairs(items) do
           universe:GiveAPItemToPlayers(v["item"])
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
end

function APClient:getLocation(location_id)
    self.ap:LocationChecks({location_id})
end

function APClient:WinGame()
    self.ap:StatusUpdate(30)
end

function APClient:poll()
    if self.running then
        self.ap:poll()
    end
end

