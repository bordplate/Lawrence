-- Load modules needed
local proto = Proto("mp_packet", "Multiplayer Packet Protocol")

-- Define fields
local packet_types = {
    [1] = "CONNECT",
    [2] = "SYN",
    [3] = "ACK",
    [4] = "MOBY_UPDATE",
    [5] = "IDKU",
    [6] = "MOBY_CREATE",
    [7] = "DISCONNECT",
    [8] = "MOBY_DELETE",
    [9] = "MOBY_COLLISION",
    [10] = "SET_STATE",
    [11] = "SET_HUD_TEXT",
    [12] = "QUERY_GAME_SERVERS",
    [13] = "CONTROLLER_INPUT",
    [14] = "TIME_SYNC",
    [15] = "PLAYER_RESPAWNED",
    [16] = "REGISTER_SERVER",
    [17] = "TOAST_MESSAGE",
    [18] = "MOBY_EXTENDED",
    [21] = "ERROR_MESSAGE"
}

function proto.dissector(buffer, pinfo, tree)
    offset = 0
    while buffer:len() - offset >= 18 do -- Loop while there are enough bytes for another packet
        pinfo.cols.protocol = proto.name
        local subtree = tree:add(proto, buffer(), "Multiplayer Packet Data")

        -- Read MPPacketHeader
        local type = buffer(offset,2):uint()
        subtree:add(buffer(offset,2), "Type: " .. packet_types[type])
        subtree:add(buffer(offset+2,2), "Flags: " .. buffer(offset+2,2):uint())
        local size = buffer(offset+4,4):uint()
        subtree:add(buffer(offset+4,4), "Size: " .. size)
        subtree:add(buffer(offset+8,8), "Time Sent: " .. buffer(offset+8,8):uint64())
        subtree:add(buffer(offset+16,1), "Requires ACK: " .. buffer(offset+16,1):uint())
        subtree:add(buffer(offset+17,1), "ACK Cycle: " .. buffer(offset+17,1):uint())

        offset = offset + 18 -- Move offset past the header

        local idx = offset

        if type == 1 then -- CONNECT
            subtree:add(buffer(idx,4), "Userid: " .. buffer(idx, 4):int())
            idx = idx + 4
            subtree:add(buffer(idx,8), "Passcode: " .. buffer(idx,8):string())
            idx = idx + 8
            nick_length = buffer(idx,2):uint()
            subtree:add(buffer(idx,2), "Nick length: " .. buffer(idx,2):uint())
            idx = idx + 2
            subtree:add(buffer(idx, nick_length), "Nickname: " .. buffer(idx,nick_length):string())

        elseif type == 2 or type == 3 then -- SYN, ACK
            -- no data for SYN, ACK

        elseif type == 4 then -- MOBY_UPDATE
            subtree:add(buffer(idx,2), "UUID: " .. buffer(idx,2):uint())
            idx = idx + 2
            subtree:add(buffer(idx,2), "Parent: " .. buffer(idx,2):uint())
            idx = idx + 2
            subtree:add(buffer(idx,2), "Flags: " .. buffer(idx,2):uint())
            idx = idx + 2
            subtree:add(buffer(idx,2), "O_class: " .. buffer(idx,2):uint())
            idx = idx + 2
            subtree:add(buffer(idx, 4), "Animation_id: " .. buffer(idx, 4):uint())
            idx = idx + 4
            -- continue in this manner for the rest of the fields

        elseif type == 5 then -- IDKU
            -- Parsing for IDKU packet type

        elseif type == 6 then -- MOBY_CREATE
            subtree:add(buffer(idx,4), "UUID: " .. buffer(idx,4):uint())
            idx = idx + 4

        elseif type == 7 then -- DISCONNECT
            -- no data for DISCONNECT

        elseif type == 8 then -- MOBY_DELETE
            -- Parsing for MOBY_DELETE packet type
            subtree:add(buffer(idx,4), "UUID: " .. buffer(idx,4):uint())

        elseif type == 9 then -- MOBY_COLLISION
            subtree:add(buffer(idx,4), "Flags: " .. buffer(idx,4):uint())
            idx = idx + 4
            subtree:add(buffer(idx,2), "UUID: " .. buffer(idx,2):uint())
            idx = idx + 2
            subtree:add(buffer(idx,2), "Collided with: " .. buffer(idx,2):uint())
            idx = idx + 2
            subtree:add(buffer(idx,4), "X: " .. buffer(idx, 4):float())
            idx = idx + 4
            -- continue in this manner for the rest of the fields

        elseif type == 10 then -- SET_STATE
            subtree:add(buffer(idx,4), "State type: " .. buffer(idx,4):uint())
            idx = idx + 4
            subtree:add(buffer(idx,4), "Offset: " .. buffer(idx,4):uint())
            idx = idx + 4
            subtree:add(buffer(idx,4), "Value: " .. buffer(idx, 4):uint())
            idx = idx + 4

        elseif type == 11 then -- SET_HUD_TEXT
            subtree:add(buffer(idx,2), "ID: " .. buffer(idx,2):uint())
            idx = idx + 2
            subtree:add(buffer(idx,2), "X: " .. buffer(idx,2):uint())
            idx = idx + 2
            subtree:add(buffer(idx,2), "Y: " .. buffer(idx,2):uint())
            idx = idx + 2
            subtree:add(buffer(idx,2), "Flags: " .. buffer(idx,2):uint())
            idx = idx + 2
            -- Add color data assumes Color stores 4 bytes with RGB values
            subtree:add(buffer(idx,1), "Red: " .. buffer(idx,1):uint())
            subtree:add(buffer(idx+1,1), "Green: " .. buffer(idx+1,1):uint())
            subtree:add(buffer(idx+2,1), "Blue: " .. buffer(idx+2,1):uint())
            subtree:add(buffer(idx+3,1), "Alpha: " .. buffer(idx+3,1):uint())
            idx = idx + 4
            subtree:add(buffer(idx,2), "Box Height: " .. buffer(idx,2):uint())
            idx = idx + 2
            subtree:add(buffer(idx, 2), "Box Width: " .. buffer(idx,2):uint())
            idx = idx + 2
            subtree:add(buffer(idx,4), "Size: " .. buffer(idx,4):float())
            idx = idx + 4
            subtree:add(buffer(idx,50), "Text: " .. buffer(idx,50):string())
            idx = idx + 50

        elseif type == 12 then -- QUERY_GAME_SERVERS
            subtree:add(buffer(idx,4), "Directory_id: " .. buffer(idx,4):uint())
            idx = idx + 4

        elseif type == 13 then -- CONTROLLER_INPUT
            subtree:add(buffer(idx, 2), "Input: " .. buffer(idx, 2):uint())
            idx = idx + 2
            subtree:add(buffer(idx, 2), "Flags: " .. buffer(idx, 2):uint())
            idx = idx + 2

        elseif type == 14 then -- TIME_SYNC 
            -- no data
        elseif type == 15 then -- PLAYER_RESPAWNED
            -- no data for PLAYER_RESPAWNED
        end

        offset = offset + size
    end
end

-- initialization routine
function proto.init()
end

-- register a chained dissector for port 2407
local udp_dissector_table = DissectorTable.get("udp.port")
dissector = udp_dissector_table:get_dissector(12345)
-- you need to change the port number to the actual port used by your protocol
udp_dissector_table:add(2407, proto)
udp_dissector_table:add(2408, proto)
