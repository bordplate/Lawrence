package.cpath = package.cpath .. ';./mods/newrando/?.dll'

local AP = require('lua-apclientpp')
assert(AP, 'Failed to load')

APClient = class("APClient")

function APClient:initialize(universe, game_name, items_handling, uuid, host, slot, password)
    local running = true  -- set this to false to kill the coroutine

    function on_socket_connected()
        print("Socket connected")
    end

    function on_socket_error(msg)
        print("Socket error: " .. msg)
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
    end

    function on_slot_refused(reasons)
       print("Slot refused: " .. table.concat(reasons, ", "))
    end

    function on_items_received(items)
       print("Archipelago Items received:")
       for _,v in ipairs(items) do
           universe:GiveItemToPlayers(v["item"])
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

function APClient:poll()
    self.ap:poll()
end

