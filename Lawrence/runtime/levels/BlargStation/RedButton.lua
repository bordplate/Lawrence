RedButton = class("RedButton", HybridMoby)

function RedButton:initialize(level, uid)
    HybridMoby.initialize(self, level, uid)

    self:MonitorAttribute(Moby.offset.state, 1)
    self:MonitorPVar(0xb * 4, 4, true)
end

function RedButton:OnAttributeChange(player, offset, oldValue, newValue)
    if offset == Moby.offset.state then
        if newValue == 3 then
            for i, other in pairs(player:Level():LuaEntity():FindChildren("Player")) do
                if player:GUID() ~= other:GUID() and player:DistanceTo(other) < 111.0 then
                    -- Change invisible management moby to update far away
                    other:ChangeMobyAttribute(150, Moby.offset.updateDistance, 1, 0xff)
                end
            end
        end

        if newValue == 4 then
            for i, other in pairs(player:Level():LuaEntity():FindChildren("Player")) do
                if player:GUID() ~= other:GUID() and player:DistanceTo(other) < 111.0 then
                    other:SetPosition(264+i, 492, 118.5)
                    other:ChangeMobyAttribute(self.UID, Moby.offset.state, 1, 4)
                    other:ChangeMobyAttribute(150, Moby.offset.state, 1, 2)

                    other:DeleteAllChildrenWithOClass(1054)
                    other:DeleteAllChildrenWithOClass(1055)
                end 
            end
        end

        if newValue == 6 then
            for i, other in pairs(player:Level():LuaEntity():FindChildren("Player")) do
                print("Distance from " .. player:Username() .. " to " .. other:Username() .. " is " .. player:DistanceTo(other))
                if player:GUID() ~= other:GUID() and player:DistanceTo(other) < 20.0 then
                    other:ChangeMobyAttribute(self.UID, Moby.offset.state, 1, 6)
                end
            end
        end

        --if newValue == 2 or newValue == 4 or newValue == 6 then
        --    self:ChangeAttributeForOtherPlayers(player, Moby.offset.state, 1, newValue)
        --end
    end
end

--function RedButton:OnPVarChange(player, offset, oldValue, newValue)
--    if offset == 0xb * 4 then
--        print("RedButton PVar from " .. oldValue .. " changed to " .. newValue .. " by " .. player:Username())
--    end
--end 