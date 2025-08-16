require 'ReplacementMobys.InfobotMoby'
require 'ReplacementMobys.GoldWeaponCaseMoby'
require 'ReplacementMobys.PlumberMoby'
require 'ReplacementMobys.TrespasserMoby'
require 'ReplacementMobys.AgentMoby'
require 'ReplacementMobys.SkidMoby'
require 'ReplacementMobys.HelgaMoby'
require 'ReplacementMobys.AlMoby'
require 'ReplacementMobys.SuckCannonMoby'
require 'ReplacementMobys.BouncerMoby'
require 'ReplacementMobys.ZoomeratorMoby'
require 'ReplacementMobys.HoverboardLadyMoby'
require 'ReplacementMobys.SalesmanMoby'
require 'ReplacementMobys.HydrodisplacerMoby'
require 'ReplacementMobys.ScientistMoby'
require 'ReplacementMobys.DeserterMoby'
require 'ReplacementMobys.CommandoMoby'
require 'ReplacementMobys.PilotHelmetMoby'
require 'ReplacementMobys.MagnebootsMoby'
require 'ReplacementMobys.NanotechVendorMoby'
require 'ReplacementMobys.BobMoby'
require 'ReplacementMobys.FredMoby'
require 'ReplacementMobys.MinerMoby'
require 'ReplacementMobys.EdwinaMoby'
require 'ReplacementMobys.SteveMoby'
require 'ReplacementMobys.SamMoby'
require 'ReplacementMobys.MorphORayMoby'
require 'ReplacementMobys.BoltGrabberMoby'
require 'ReplacementMobys.HologuiseMoby'
require 'ReplacementMobys.CodebotMoby'

ReplacementMobys = class("ReplacementMobys")

function ReplacementMobys:initialize(universe)
    self.universe = universe
    self.replacedMobys = {
        -- Novalis
        Plumber = self.universe:GetLevelByName("Novalis"):SpawnMoby(PlumberMoby),
        BombGloveCase = self.universe:GetLevelByName("Novalis"):SpawnMoby(GoldWeaponCaseMoby),
        PyrocitorCase = self.universe:GetLevelByName("Novalis"):SpawnMoby(GoldWeaponCaseMoby),
        BlasterCase = self.universe:GetLevelByName("Novalis"):SpawnMoby(GoldWeaponCaseMoby),
        GloveOfDoomCase = self.universe:GetLevelByName("Novalis"):SpawnMoby(GoldWeaponCaseMoby),
        SuckCannonCase = self.universe:GetLevelByName("Novalis"):SpawnMoby(GoldWeaponCaseMoby),
        -- Aridia
        Trespasser = self.universe:GetLevelByName("Aridia"):SpawnMoby(TrespasserMoby),
        Agent = self.universe:GetLevelByName("Aridia"):SpawnMoby(AgentMoby),
        -- Kerwan
        Helga = self.universe:GetLevelByName("Kerwan"):SpawnMoby(HelgaMoby),
        Al = self.universe:GetLevelByName("Kerwan"):SpawnMoby(AlMoby),
        KerwanInfobot = self.universe:GetLevelByName("Kerwan"):SpawnMoby(InfobotMoby),
        -- Eudora
        SuckCannon = self.universe:GetLevelByName("Eudora"):SpawnMoby(SuckCannonMoby),
        -- Rilgar
        Bouncer = self.universe:GetLevelByName("Rilgar"):SpawnMoby(BouncerMoby),
        --Zoomerator = self.universe:GetLevelByName("Rilgar"):SpawnMoby(ZoomeratorMoby),
        Salesman = self.universe:GetLevelByName("Rilgar"):SpawnMoby(SalesmanMoby),
        HoverboardLady = HoverboardLadyMoby(self.universe:GetLevelByName("Rilgar"), 662, self.universe),
        -- Blarg
        Hydrodisplacer = self.universe:GetLevelByName("BlargStation"):SpawnMoby(HydrodisplacerMoby),
        Scientist = self.universe:GetLevelByName("BlargStation"):SpawnMoby(ScientistMoby),
        -- Umbris
        UmbrisInfobot = self.universe:GetLevelByName("Umbris"):SpawnMoby(InfobotMoby),
        -- Batalia
        Deserter = self.universe:GetLevelByName("Batalia"):SpawnMoby(DeserterMoby),
        Commando = self.universe:GetLevelByName("Batalia"):SpawnMoby(CommandoMoby),
        -- Gaspar
        PilotHelmet = self.universe:GetLevelByName("Gaspar"):SpawnMoby(PilotHelmetMoby),
        -- Orxon
        Magneboots = self.universe:GetLevelByName("Orxon"):SpawnMoby(MagnebootsMoby),
        NanotechVendor = self.universe:GetLevelByName("Orxon"):SpawnMoby(NanotechVendorMoby),
        OrxonClankInfobot = self.universe:GetLevelByName("Orxon"):SpawnMoby(InfobotMoby),
        OrxonRatchetInfobot = self.universe:GetLevelByName("Orxon"):SpawnMoby(InfobotMoby),
        -- Pokitaru
        Bob = self.universe:GetLevelByName("Pokitaru"):SpawnMoby(BobMoby),
        Fred = self.universe:GetLevelByName("Pokitaru"):SpawnMoby(FredMoby),
        -- Hoven
        Miner = self.universe:GetLevelByName("Hoven"):SpawnMoby(MinerMoby),
        Edwina = self.universe:GetLevelByName("Hoven"):SpawnMoby(EdwinaMoby),
        -- Gemlik
        TeslaClawCase = self.universe:GetLevelByName("GemlikStation"):SpawnMoby(GoldWeaponCaseMoby),
        DevastatorCase = self.universe:GetLevelByName("GemlikStation"):SpawnMoby(GoldWeaponCaseMoby),
        MineGloveCase = self.universe:GetLevelByName("GemlikStation"):SpawnMoby(GoldWeaponCaseMoby),
        MorphORayCase = self.universe:GetLevelByName("GemlikStation"):SpawnMoby(GoldWeaponCaseMoby),
        DecoyGloveCase = self.universe:GetLevelByName("GemlikStation"):SpawnMoby(GoldWeaponCaseMoby),
        -- Oltanis
        Steve = self.universe:GetLevelByName("Oltanis"):SpawnMoby(SteveMoby),
        Sam = self.universe:GetLevelByName("Oltanis"):SpawnMoby(SamMoby),
        MorphORay = self.universe:GetLevelByName("Oltanis"):SpawnMoby(MorphORayMoby),
        -- Quartu
        BoltGrabber = self.universe:GetLevelByName("Quartu"):SpawnMoby(BoltGrabberMoby),
        -- Kalebo III
        Hologuise = self.universe:GetLevelByName("KaleboIII"):SpawnMoby(HologuiseMoby),
        -- Drek's Fleet
        Codebot = self.universe:GetLevelByName("DreksFleet"):SpawnMoby(CodebotMoby),
        FleetInfobot = self.universe:GetLevelByName("DreksFleet"):SpawnMoby(InfobotMoby),
    }

    -- change values of generic mobys
    self.replacedMobys.BombGloveCase:SetPosition(241.223, 128.536, 54.497)
    self.replacedMobys.BombGloveCase.rotZ = -0.475
    self.replacedMobys.BombGloveCase.bolt_cost = 20000
    self.replacedMobys.BombGloveCase.gold_bolt_cost = 4
    self.replacedMobys.BombGloveCase.item_name = "Golden Bomb Glove"
    self.replacedMobys.BombGloveCase.item_id = 400

    self.replacedMobys.PyrocitorCase:SetPosition(239.557, 139.003, 54.497)
    self.replacedMobys.PyrocitorCase.rotZ = -0.475
    self.replacedMobys.PyrocitorCase.bolt_cost = 30000
    self.replacedMobys.PyrocitorCase.gold_bolt_cost = 4
    self.replacedMobys.PyrocitorCase.item_name = "Golden Pyrocitor"
    self.replacedMobys.PyrocitorCase.item_id = 401

    self.replacedMobys.BlasterCase:SetPosition(247.015, 145.090, 54.497)
    self.replacedMobys.BlasterCase.rotZ = -0.475
    self.replacedMobys.BlasterCase.bolt_cost = 20000
    self.replacedMobys.BlasterCase.gold_bolt_cost = 4
    self.replacedMobys.BlasterCase.item_name = "Golden Blaster"
    self.replacedMobys.BlasterCase.item_id = 402

    self.replacedMobys.GloveOfDoomCase:SetPosition(257.627, 144.131, 54.497)
    self.replacedMobys.GloveOfDoomCase.rotZ = -0.475
    self.replacedMobys.GloveOfDoomCase.bolt_cost = 10000
    self.replacedMobys.GloveOfDoomCase.gold_bolt_cost = 4
    self.replacedMobys.GloveOfDoomCase.item_name = "Golden Glove Of Doom"
    self.replacedMobys.GloveOfDoomCase.item_id = 403

    self.replacedMobys.SuckCannonCase:SetPosition(261.73, 134.362, 54.497)
    self.replacedMobys.SuckCannonCase.rotZ = -0.475
    self.replacedMobys.SuckCannonCase.bolt_cost = 10000
    self.replacedMobys.SuckCannonCase.gold_bolt_cost = 4
    self.replacedMobys.SuckCannonCase.item_name = "Golden Suck Cannon"
    self.replacedMobys.SuckCannonCase.item_id = 404
    
    self.replacedMobys.TeslaClawCase:SetPosition(425.060, 580.726, 401.992)
    self.replacedMobys.TeslaClawCase.rotZ = -0.475
    self.replacedMobys.TeslaClawCase.bolt_cost = 60000
    self.replacedMobys.TeslaClawCase.gold_bolt_cost = 4
    self.replacedMobys.TeslaClawCase.item_name = "Golden Tesla Claw"
    self.replacedMobys.TeslaClawCase.item_id = 405

    self.replacedMobys.DevastatorCase:SetPosition(418.738, 589.229, 401.992)
    self.replacedMobys.DevastatorCase.rotZ = -0.475
    self.replacedMobys.DevastatorCase.bolt_cost = 60000
    self.replacedMobys.DevastatorCase.gold_bolt_cost = 4
    self.replacedMobys.DevastatorCase.item_name = "Golden Devastator"
    self.replacedMobys.DevastatorCase.item_id = 406

    self.replacedMobys.MineGloveCase:SetPosition(422.145, 599.232, 401.992)
    self.replacedMobys.MineGloveCase.rotZ = -0.475
    self.replacedMobys.MineGloveCase.bolt_cost = 10000
    self.replacedMobys.MineGloveCase.gold_bolt_cost = 4
    self.replacedMobys.MineGloveCase.item_name = "Golden Mine Glove"
    self.replacedMobys.MineGloveCase.item_id = 407

    self.replacedMobys.MorphORayCase:SetPosition(432.337, 602.158, 401.992)
    self.replacedMobys.MorphORayCase.rotZ = -0.475
    self.replacedMobys.MorphORayCase.bolt_cost = 60000
    self.replacedMobys.MorphORayCase.gold_bolt_cost = 4
    self.replacedMobys.MorphORayCase.item_name = "Golden Morph O Ray"
    self.replacedMobys.MorphORayCase.item_id = 408

    self.replacedMobys.DecoyGloveCase:SetPosition(440.510, 595.414, 401.992)
    self.replacedMobys.DecoyGloveCase.rotZ = -0.475
    self.replacedMobys.DecoyGloveCase.bolt_cost = 10000
    self.replacedMobys.DecoyGloveCase.gold_bolt_cost = 4
    self.replacedMobys.DecoyGloveCase.item_name = "Golden Decoy Glove"
    self.replacedMobys.DecoyGloveCase.item_id = 409
    
    self.replacedMobys.KerwanInfobot:SetPosition(288.260, 127.805, 65.109)
    self.replacedMobys.KerwanInfobot.rotZ = 1.571
    self.replacedMobys.KerwanInfobot.planet_id = 0x04

    self.replacedMobys.UmbrisInfobot:SetPosition(216, 455, 36.5)
    self.replacedMobys.UmbrisInfobot.rotZ = 1.571
    self.replacedMobys.UmbrisInfobot.planet_id = 0x08

    self.replacedMobys.OrxonClankInfobot:SetPosition(238, 190, 59.5)
    self.replacedMobys.OrxonClankInfobot.rotZ = 1.571
    self.replacedMobys.OrxonClankInfobot.planet_id = 0x0b
    self.replacedMobys.OrxonRatchetInfobot:SetPosition(307, 228, 68.5)
    self.replacedMobys.OrxonRatchetInfobot.rotZ = 1.571
    self.replacedMobys.OrxonRatchetInfobot.planet_id = 0x0c
    
    self.replacedMobys.FleetInfobot:SetPosition(695, 521, 169)
    self.replacedMobys.FleetInfobot.rotZ = 1.571
    self.replacedMobys.FleetInfobot.planet_id = 0x12
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
        
        player:DeleteAllChildrenWithUID(909) -- Gold Tesla Claw Case
        player:DeleteAllChildrenWithUID(908) -- Gold Bomb Glove Case
        player:DeleteAllChildrenWithUID(907) -- Gold Devastator Case
        player:DeleteAllChildrenWithUID(906) -- Gold Pyrocitor Case
        player:DeleteAllChildrenWithUID(905) -- Gold Mine Glove Case
        player:DeleteAllChildrenWithUID(904) -- Gold Blaster Case
        player:DeleteAllChildrenWithUID(903) -- Gold Morph-O-Ray Case
        player:DeleteAllChildrenWithUID(902) -- Gold Glove Of Doom Case
        player:DeleteAllChildrenWithUID(901) -- Gold Decoy Glove Case
        player:DeleteAllChildrenWithUID(900) -- Gold Suck Cannon Case
        
        player:DeleteAllChildrenWithUID(897) -- Gold Tesla Claw
        player:DeleteAllChildrenWithUID(1222) -- Gold Bomb Glove
        player:DeleteAllChildrenWithUID(899) -- Gold Devastator
        player:DeleteAllChildrenWithUID(893) -- Gold Pyrocitor
        player:DeleteAllChildrenWithUID(898) -- Gold Mine Glove
        player:DeleteAllChildrenWithUID(894) -- Gold Blaster
        player:DeleteAllChildrenWithUID(910) -- Gold Morph-O-Ray
        player:DeleteAllChildrenWithUID(895) -- Gold Glove Of Doom
        player:DeleteAllChildrenWithUID(911) -- Gold Decoy Glove
        player:DeleteAllChildrenWithUID(896) -- Gold Suck Cannon
    elseif levelName == "Aridia" then
        player:DeleteAllChildrenWithUID(564) -- Trespasser
        player:DeleteAllChildrenWithUID(419) -- Skid's Agent
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
    elseif levelName == "Batalia" then
        player:DeleteAllChildrenWithUID(629) -- Deserter
        player:DeleteAllChildrenWithUID(605) -- Commando
    elseif levelName == "Gaspar" then
        player:DeleteAllChildrenWithUID(326) -- Pilot's Helmet
    elseif levelName == "Orxon" then
        player:DeleteAllChildrenWithUID(742) -- Magneboots
        player:DeleteAllChildrenWithUID(749) -- Nanotech Vendor
        player:DeleteAllChildrenWithUID(255) -- Clank Infobot
        player:DeleteAllChildrenWithUID(256) -- Ratchet Infobot
    elseif levelName == "Pokitaru" then
        player:DeleteAllChildrenWithUID(653) -- Bob
        player:DeleteAllChildrenWithUID(652) -- Fred
    elseif levelName == "Hoven" then
        player:DeleteAllChildrenWithUID(649) -- Miner
        player:DeleteAllChildrenWithUID(67) -- Edwina
    elseif levelName == "GemlikStation" then
        player:DeleteAllChildrenWithUID(909) -- Gold Tesla Claw Case
        player:DeleteAllChildrenWithUID(908) -- Gold Bomb Glove Case
        player:DeleteAllChildrenWithUID(907) -- Gold Devastator Case
        player:DeleteAllChildrenWithUID(906) -- Gold Pyrocitor Case
        player:DeleteAllChildrenWithUID(905) -- Gold Mine Glove Case
        player:DeleteAllChildrenWithUID(904) -- Gold Blaster Case
        player:DeleteAllChildrenWithUID(903) -- Gold Morph-O-Ray Case
        player:DeleteAllChildrenWithUID(902) -- Gold Glove Of Doom Case
        player:DeleteAllChildrenWithUID(901) -- Gold Decoy Glove Case
        player:DeleteAllChildrenWithUID(900) -- Gold Suck Cannon Case

        player:DeleteAllChildrenWithUID(897) -- Gold Tesla Claw
        player:DeleteAllChildrenWithUID(265) -- Gold Bomb Glove
        player:DeleteAllChildrenWithUID(899) -- Gold Devastator
        player:DeleteAllChildrenWithUID(893) -- Gold Pyrocitor
        player:DeleteAllChildrenWithUID(898) -- Gold Mine Glove
        player:DeleteAllChildrenWithUID(894) -- Gold Blaster
        player:DeleteAllChildrenWithUID(910) -- Gold Morph-O-Ray
        player:DeleteAllChildrenWithUID(895) -- Gold Glove Of Doom
        player:DeleteAllChildrenWithUID(911) -- Gold Decoy Glove
        player:DeleteAllChildrenWithUID(896) -- Gold Suck Cannon
    elseif levelName == "Oltanis" then
        player:DeleteAllChildrenWithUID(360) -- Steve
        player:DeleteAllChildrenWithUID(25) -- Sam
        player:DeleteAllChildrenWithUID(414) -- Morph-o-Ray
    elseif levelName == "Quartu" then
        player:DeleteAllChildrenWithUID(365) -- Bolt Grabber
    elseif levelName == "DreksFleet" then
        player:DeleteAllChildrenWithUID(669) -- Codebot
        player:DeleteAllChildrenWithUID(285) -- Infobot
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