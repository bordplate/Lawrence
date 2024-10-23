HybridMoby = class("HybridMoby", Moby)

function HybridMoby:initialize(level, uid)
    mobyEntity = level:GetGameMobyByUID(uid)
    mobyEntity:SetLuaEntity(self)
    Moby.initialize(self, mobyEntity)
end

function HybridMoby:ChangeAttributeForOtherPlayers(player, offset, size, newValue, isFloat)
    isFloat = isFloat or false
    
    for _, other in pairs(self:Level():FindChildren("Player")) do
        if player:GUID() ~= other:GUID() then
            other:ChangeMobyAttribute(self.UID, offset, size, newValue, isFloat)
        end
    end
end

function HybridMoby:ChangePVarForOtherPlayers(player, offset, size, newValue, isFloat)
    isFloat = isFloat or false
    
    for _, other in pairs(self:Level():FindChildren("Player")) do
        if player:GUID() ~= other:GUID() then
            other:ChangeMobyPVar(self.UID, offset, size, newValue, isFloat)
        end
    end
end