require 'ReplacementMobys.HelgaMoby'
require 'ReplacementMobys.AlMoby'
require 'ReplacementMobys.BobMoby'

ReplacementMobys = class("ReplacementMobys")

function ReplacementMobys:initialize(universe)
    self.universe = universe
    self.replacedMobys = {
        helga = self.universe:GetLevelByName("Kerwan"):SpawnMoby(HelgaMoby),
        al = self.universe:GetLevelByName("Kerwan"):SpawnMoby(AlMoby),
        bob = self.universe:GetLevelByName("Pokitaru"):SpawnMoby(BobMoby),
    }
end

function ReplacementMobys:Triangle(player)
    for _, moby in pairs(self.replacedMobys) do
        if moby ~= nil then
            moby:Triangle(player, self)
        end
    end
end

function ReplacementMobys:ToastMessage(player)
    for _, moby in pairs(self.replacedMobys) do
        if moby ~= nil then
            moby:ToastMessage(player)
        end
    end
end

function ReplacementMobys:RemoveReplacedMobys(player)
    if player:Level():GetName() == "Kerwan" then
        player:DeleteAllChildrenWithUID(158) -- Helga
        player:DeleteAllChildrenWithUID(165) -- Al
    end

    if player:Level():GetName() == "Pokitaru" then
        player:DeleteAllChildrenWithUID(653) -- Bob
    end
end

