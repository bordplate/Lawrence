require 'ReplacementMobys.InfobotMoby'
require 'ReplacementMobys.PlumberMoby'
require 'ReplacementMobys.TrespasserMoby'
require 'ReplacementMobys.AgentMoby'
require 'ReplacementMobys.SkidMoby'
require 'ReplacementMobys.HelgaMoby'
require 'ReplacementMobys.AlMoby'
require 'ReplacementMobys.BobMoby'

ReplacementMobys = class("ReplacementMobys")

function ReplacementMobys:initialize(universe)
    self.universe = universe
    self.replacedMobys = {
        -- Novalis
        Plumber = self.universe:GetLevelByName("Novalis"):SpawnMoby(PlumberMoby),
        -- Aridia
        Trespasser = self.universe:GetLevelByName("Aridia"):SpawnMoby(TrespasserMoby),
        Agent = self.universe:GetLevelByName("Aridia"):SpawnMoby(AgentMoby),
        Skid = self.universe:GetLevelByName("Aridia"):SpawnMoby(SkidMoby),
        -- Kerwan
        Helga = self.universe:GetLevelByName("Kerwan"):SpawnMoby(HelgaMoby),
        Al = self.universe:GetLevelByName("Kerwan"):SpawnMoby(AlMoby),
        KerwanInfobot = self.universe:GetLevelByName("Kerwan"):SpawnMoby(InfobotMoby),
        -- Pokitaru
        Bob = self.universe:GetLevelByName("Pokitaru"):SpawnMoby(BobMoby),
    }
    
    -- change values of generic mobys
    self.replacedMobys.KerwanInfobot:SetPosition(288, 128, 66)
    self.replacedMobys.KerwanInfobot.planet_id = 0x04
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
    if player:Level():GetName() == "Novalis" then
        player:DeleteAllChildrenWithUID(558) -- Plumber
    end
    if player:Level():GetName() == "Aridia" then
        player:DeleteAllChildrenWithUID(564) -- Trespasser
        player:DeleteAllChildrenWithUID(419) -- Skid's Agent
        player:DeleteAllChildrenWithUID(532) -- Skid
    end
    if player:Level():GetName() == "Kerwan" then
        player:DeleteAllChildrenWithUID(158) -- Helga
        player:DeleteAllChildrenWithUID(165) -- Al
        player:DeleteAllChildrenWithUID(60) -- Infobot
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
