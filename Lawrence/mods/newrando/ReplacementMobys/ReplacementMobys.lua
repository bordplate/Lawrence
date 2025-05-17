require 'ReplacementMobys.InfobotMoby'
require 'ReplacementMobys.PlumberMoby'
require 'ReplacementMobys.TrespasserMoby'
require 'ReplacementMobys.AgentMoby'
require 'ReplacementMobys.SkidMoby'
require 'ReplacementMobys.HelgaMoby'
require 'ReplacementMobys.AlMoby'
require 'ReplacementMobys.SuckCannonMoby'
require 'ReplacementMobys.BouncerMoby'
require 'ReplacementMobys.ZoomeratorMoby'
require 'ReplacementMobys.SalesmanMoby'
require 'ReplacementMobys.HydrodisplacerMoby'
require 'ReplacementMobys.ScientistMoby'
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
        -- Eudora
        SuckCannon = self.universe:GetLevelByName("Eudora"):SpawnMoby(SuckCannonMoby),
        -- Rilgar
        Bouncer = self.universe:GetLevelByName("Rilgar"):SpawnMoby(BouncerMoby),
        Zoomerator = self.universe:GetLevelByName("Rilgar"):SpawnMoby(ZoomeratorMoby),
        Salesman = self.universe:GetLevelByName("Rilgar"):SpawnMoby(SalesmanMoby),
        -- Blarg
        Hydrodisplacer = self.universe:GetLevelByName("BlargStation"):SpawnMoby(HydrodisplacerMoby),
        Scientist = self.universe:GetLevelByName("BlargStation"):SpawnMoby(ScientistMoby),
        -- Umbris
        UmbrisInfobot = self.universe:GetLevelByName("Umbris"):SpawnMoby(InfobotMoby),
        -- Pokitaru
        Bob = self.universe:GetLevelByName("Pokitaru"):SpawnMoby(BobMoby),
    }
    
    -- change values of generic mobys
    self.replacedMobys.KerwanInfobot:SetPosition(288, 128, 66)
    self.replacedMobys.KerwanInfobot.planet_id = 0x04

    self.replacedMobys.UmbrisInfobot:SetPosition(216, 455, 36.5)
    self.replacedMobys.UmbrisInfobot.planet_id = 0x08
    
    
end

function ReplacementMobys:Triangle(player)
    for _, moby in pairs(self.replacedMobys) do
        if moby ~= nil then
            moby:Triangle(player, self.universe)
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
    levelName = player:Level():GetName()
    if levelName == "Novalis" then
        player:DeleteAllChildrenWithUID(558) -- Plumber
    elseif levelName == "Aridia" then
        player:DeleteAllChildrenWithUID(564) -- Trespasser
        player:DeleteAllChildrenWithUID(419) -- Skid's Agent
        player:DeleteAllChildrenWithUID(532) -- Skid
    elseif levelName == "Kerwan" then
        player:DeleteAllChildrenWithUID(158) -- Helga
        player:DeleteAllChildrenWithUID(165) -- Al
        player:DeleteAllChildrenWithUID(60) -- Infobot    
    elseif levelName == "Eudora" then
        player:DeleteAllChildrenWithUID(449) -- Suck cannon
    elseif levelName == "Rilgar" then
        player:DeleteAllChildrenWithUID(661) -- Bouncer
        player:DeleteAllChildrenWithUID(1334) -- Shady Salesman
    elseif levelName == "BlargStation" then
        player:DeleteAllChildrenWithUID(191) -- Hydrodisplacer
        player:DeleteAllChildrenWithUID(186) -- Scientist
    elseif levelName == "Umbris" then
        player:DeleteAllChildrenWithUID(445) -- unnamed object that causes travel to batalia
    elseif levelName == "Pokitaru" then
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
