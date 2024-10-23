Item = class("Item")

function Item:initialize(name, id, isWeapon)
    self.name = name
    self.id = id
    self.isWeapon = isWeapon
end

function Item.GetByName(name)
    for _, item in ipairs(ITEMS) do
        if item[1] == name then
            return Item:new(item[1], item[2], item[3])
        end
    end
end

function Item.GetById(id)
    for _, item in ipairs(ITEMS) do
        if item[2] == id then
            return Item:new(item[1], item[2], item[3])
        end
    end
end

--  -- packs
--        heli_pack           ={0x02, true},
--        thruster_pack       ={0x03, true},
--        hydro_pack          ={0x04, true},
--    -- helmets
--        sonic_summoner      ={0x05, true},
--        o2_mask             ={0x06, true},
--        pilots_helmet       ={0x07, true},
--    -- gadgets
--        swingshot           ={0x0c, true},
--        hydrodisplacer      ={0x16, true},
--        trespasser          ={0x1a, true},
--        metal_detector      ={0x1b, true},
--        hologuise           ={0x1f, true},
--        pda                 ={0x20, true},
--    -- boots
--        magneboots          ={0x1c, true},
--        grindboots          ={0x1d, true},
--    -- items
--        hoverboard          ={0x1e, false},
--        map_o_matic         ={0x21, false},
--        bolt_grabber        ={0x22, false},
--        persuader           ={0x23, false},
--        zoomerator          ={0x30, false},
--        raritanium          ={0x31, false},
--        codebot             ={0x32, false},
--        premium_nanotech    ={0x34, false},
--        ultra_nanotech      ={0x35, false},
--    -- vendor weapons
--        devastator          ={0x0b, true},
--        visibomb            ={0x0d, true},
--        taunter             ={0x0e, true},
--        blaster             ={0x0f, true},
--        pyrociter           ={0x10, true},
--        mine_glove          ={0x11, true},
--        walloper            ={0x12, true},
--        tesla_claw          ={0x13, true},
--        glove_of_doom       ={0x14, true},
--        drone_device        ={0x18, true},
--        decoy_glove         ={0x19, true},
--    -- check weapons
--        bomb_glove          ={0x0a, true},
--        suck_cannon         ={0x09, true},
--        morph_o_ray         ={0x15, true},
--        ryno                ={0x17, true},

ITEMS = {
    {"Heli-pack", 0x2, false},
    {"Thruster-pack", 0x3, false},
    {"Hydro-pack", 0x4, false},
    {"Sonic Summoner", 0x5, false},
    {"O2 Mask", 0x6, false},
    {"Pilot's Helmet", 0x7, false},
    {"Swingshot", 0xc, false},
    {"Hydrodisplacer", 0x16, false},
    {"Trespasser", 0x1a, false},
    {"Metal Detector", 0x1b, false},
    {"Hologuise", 0x1f, false},
    {"PDA", 0x20, false},
    {"Magneboots", 0x1c, false},
    {"Grindboots", 0x1d, false},
    {"Hoverboard", 0x1e, false},
    {"Map-o-Matic", 0x21, false},
    {"Bolt Grabber", 0x22, false},
    {"Persuader", 0x23, false},
    {"Zoomerator", 0x30, false},
    {"Raritanium", 0x31, false},
    {"Codebot", 0x32, false},
    {"Premium Nanotech", 0x34, false},
    {"Ultra Nanotech", 0x35, false},
    {"Devastator", 0x0b, true},
    {"Visibomb", 0x0d, true},
    {"Taunter", 0x0e, true},
    {"Blaster", 0x0f, true},
    {"Pyrociter", 0x10, true},
    {"Mine Glove", 0x11, true},
    {"Walloper", 0x12, true},
    {"Tesla Claw", 0x13, true},
    {"Glove of Doom", 0x14, true},
    {"Drone Device", 0x18, true},
    {"Decoy Glove", 0x19, true},
    {"Bomb Glove", 0x0a, true},
    {"Suck Cannon", 0x09, true},
    {"Morph-o-Ray", 0x15, true},
    {"R.Y.N.O.", 0x17, true}
}