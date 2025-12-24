require 'ReplacementMobys.GoldWeaponCaseMoby'

ReplacementMobys = class("ReplacementMobys")

function ReplacementMobys:initialize(universe)
    self.universe = universe
    self.replacedMobys = {
        -- Novalis
        BombGloveCase = self.universe:GetLevelByName("Novalis"):SpawnMoby(GoldWeaponCaseMoby),
        PyrocitorCase = self.universe:GetLevelByName("Novalis"):SpawnMoby(GoldWeaponCaseMoby),
        BlasterCase = self.universe:GetLevelByName("Novalis"):SpawnMoby(GoldWeaponCaseMoby),
        GloveOfDoomCase = self.universe:GetLevelByName("Novalis"):SpawnMoby(GoldWeaponCaseMoby),
        SuckCannonCase = self.universe:GetLevelByName("Novalis"):SpawnMoby(GoldWeaponCaseMoby),
        TeslaClawCase = self.universe:GetLevelByName("Novalis"):SpawnMoby(GoldWeaponCaseMoby),
        DevastatorCase = self.universe:GetLevelByName("Novalis"):SpawnMoby(GoldWeaponCaseMoby),
        MineGloveCase = self.universe:GetLevelByName("Novalis"):SpawnMoby(GoldWeaponCaseMoby),
        MorphORayCase = self.universe:GetLevelByName("Novalis"):SpawnMoby(GoldWeaponCaseMoby),
        DecoyGloveCase = self.universe:GetLevelByName("Novalis"):SpawnMoby(GoldWeaponCaseMoby),
    }

    -- change values of generic mobys
    self.replacedMobys.BombGloveCase:SetPosition(241.223, 128.536, 54.497)
    self.replacedMobys.BombGloveCase.rotZ = -54.717
    self.replacedMobys.BombGloveCase.bolt_cost = 20000
    self.replacedMobys.BombGloveCase.gold_bolt_cost = 4
    self.replacedMobys.BombGloveCase.item_name = "Golden Bomb Glove"
    self.replacedMobys.BombGloveCase.item_id = 400
    self.replacedMobys.BombGloveCase:AttachWeapon(1456, self.universe:GetLevelByName("Novalis"))

    self.replacedMobys.PyrocitorCase:SetPosition(239.557, 139.003, 54.497)
    self.replacedMobys.PyrocitorCase.rotZ = -107.314
    self.replacedMobys.PyrocitorCase.bolt_cost = 30000
    self.replacedMobys.PyrocitorCase.gold_bolt_cost = 4
    self.replacedMobys.PyrocitorCase.item_name = "Golden Pyrocitor"
    self.replacedMobys.PyrocitorCase.item_id = 401
    self.replacedMobys.PyrocitorCase:AttachWeapon(1457, self.universe:GetLevelByName("Novalis"))

    self.replacedMobys.BlasterCase:SetPosition(247.015, 145.090, 54.497)
    self.replacedMobys.BlasterCase.rotZ = -161.230
    self.replacedMobys.BlasterCase.bolt_cost = 20000
    self.replacedMobys.BlasterCase.gold_bolt_cost = 4
    self.replacedMobys.BlasterCase.item_name = "Golden Blaster"
    self.replacedMobys.BlasterCase.item_id = 402
    self.replacedMobys.BlasterCase:AttachWeapon(1458, self.universe:GetLevelByName("Novalis"))

    self.replacedMobys.GloveOfDoomCase:SetPosition(257.627, 144.131, 54.497)
    self.replacedMobys.GloveOfDoomCase.rotZ = 144.270
    self.replacedMobys.GloveOfDoomCase.bolt_cost = 10000
    self.replacedMobys.GloveOfDoomCase.gold_bolt_cost = 4
    self.replacedMobys.GloveOfDoomCase.item_name = "Golden Glove Of Doom"
    self.replacedMobys.GloveOfDoomCase.item_id = 403
    self.replacedMobys.GloveOfDoomCase:AttachWeapon(1459, self.universe:GetLevelByName("Novalis"))

    self.replacedMobys.SuckCannonCase:SetPosition(261.73, 134.362, 54.497)
    self.replacedMobys.SuckCannonCase.rotZ = 144.270
    self.replacedMobys.SuckCannonCase.bolt_cost = 10000
    self.replacedMobys.SuckCannonCase.gold_bolt_cost = 4
    self.replacedMobys.SuckCannonCase.item_name = "Golden Suck Cannon"
    self.replacedMobys.SuckCannonCase.item_id = 404
    self.replacedMobys.SuckCannonCase:AttachWeapon(1464, self.universe:GetLevelByName("Novalis"))
    
    self.replacedMobys.TeslaClawCase:SetPosition(245.432, 125.064, 54.497)
    self.replacedMobys.TeslaClawCase.rotZ = -27.21
    self.replacedMobys.TeslaClawCase.bolt_cost = 60000
    self.replacedMobys.TeslaClawCase.gold_bolt_cost = 4
    self.replacedMobys.TeslaClawCase.item_name = "Golden Tesla Claw"
    self.replacedMobys.TeslaClawCase.item_id = 405
    self.replacedMobys.TeslaClawCase:AttachWeapon(1463, self.universe:GetLevelByName("Novalis"))

    self.replacedMobys.DevastatorCase:SetPosition(239.110, 133.567, 54.497)
    self.replacedMobys.DevastatorCase.rotZ = -83.651
    self.replacedMobys.DevastatorCase.bolt_cost = 60000
    self.replacedMobys.DevastatorCase.gold_bolt_cost = 4
    self.replacedMobys.DevastatorCase.item_name = "Golden Devastator"
    self.replacedMobys.DevastatorCase.item_id = 406
    self.replacedMobys.DevastatorCase:AttachWeapon(1461, self.universe:GetLevelByName("Novalis"))

    self.replacedMobys.MineGloveCase:SetPosition(242.517, 143.600, 54.497)
    self.replacedMobys.MineGloveCase.rotZ = -107.314
    self.replacedMobys.MineGloveCase.bolt_cost = 10000
    self.replacedMobys.MineGloveCase.gold_bolt_cost = 4
    self.replacedMobys.MineGloveCase.item_name = "Golden Mine Glove"
    self.replacedMobys.MineGloveCase.item_id = 407
    self.replacedMobys.MineGloveCase:AttachWeapon(1460, self.universe:GetLevelByName("Novalis"))

    self.replacedMobys.MorphORayCase:SetPosition(252.468, 145.282, 54.497)
    self.replacedMobys.MorphORayCase.rotZ = -161.230
    self.replacedMobys.MorphORayCase.bolt_cost = 60000
    self.replacedMobys.MorphORayCase.gold_bolt_cost = 4
    self.replacedMobys.MorphORayCase.item_name = "Golden Morph O Ray"
    self.replacedMobys.MorphORayCase.item_id = 408
    self.replacedMobys.MorphORayCase:AttachWeapon(1465, self.universe:GetLevelByName("Novalis"))

    self.replacedMobys.DecoyGloveCase:SetPosition(260.882, 139.753, 54.497)
    self.replacedMobys.DecoyGloveCase.rotZ = 144.270
    self.replacedMobys.DecoyGloveCase.bolt_cost = 10000
    self.replacedMobys.DecoyGloveCase.gold_bolt_cost = 4
    self.replacedMobys.DecoyGloveCase.item_name = "Golden Decoy Glove"
    self.replacedMobys.DecoyGloveCase.item_id = 409
    self.replacedMobys.DecoyGloveCase:AttachWeapon(1462, self.universe:GetLevelByName("Novalis"))
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