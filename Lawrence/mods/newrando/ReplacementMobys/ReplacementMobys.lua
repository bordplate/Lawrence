require 'ReplacementMobys.HelgaMoby'
require 'ReplacementMobys.AlMoby'
require 'ReplacementMobys.BobMoby'

ReplacementMobys = class("ReplacementMobys")

function ReplacementMobys:initialize(universe)
    self.universe = universe
    self.replacedMobys = {
        Helga = self.universe:GetLevelByName("Kerwan"):SpawnMoby(HelgaMoby),
        Al = self.universe:GetLevelByName("Kerwan"):SpawnMoby(AlMoby),
        Bob = self.universe:GetLevelByName("Pokitaru"):SpawnMoby(BobMoby),
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

DummyMoby = class("DummyMoby")

function DummyMoby:Disable()
    -- nothing, it's a dummy
    print('dummy disable')
end

function ReplacementMobys:GetMoby(name)
    if self.replacedMobys[name] ~= nil then
        return self.replacedMobys[name]
    else
        return DummyMoby()
    end
end
