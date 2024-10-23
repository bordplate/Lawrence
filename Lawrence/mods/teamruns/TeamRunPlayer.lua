TeamRunPlayer = class("TeamRunPlayer", Player)

function TeamRunPlayer:Made()
    self.damageCooldown = 0
    self.goldBoltCount = 0
end

function TeamRunPlayer:OnCollectedGoldBolt(planet, number)
    print("Player collected gold bolt on " .. planet .. " number: " .. number);

    self:Parent():BlockGoldBolt(planet, number)

    self.universe.blocked_bolts[#self.universe.blocked_bolts+1] = {planet, number}

    self.goldBoltCount = self.goldBoltCount + 1
end

function TeamRunPlayer:OnAttack(moby)
    if self.damageCooldown <= 0 then
        moby:Damage(1)
        self.damageCooldown = 40
    end
end

function TeamRunPlayer:Unfreeze()
    self.state = 0
end

function TeamRunPlayer:OnTick()
    if (self.damageCooldown > 0) then
        self.damageCooldown = self.damageCooldown - 1
    end
end

function TeamRunPlayer:OnUnlockItem(item_id, equip)
    item = Item.GetById(item_id)
    
    print("Unlocking item " .. item.name)

    self:GiveItem(item.id, equip)

    for _, player in ipairs(self:Universe():LuaEntity():FindChildren("Player")) do
        if player:Username() ~= self:Username() then
            if item.isWeapon then
                player:ToastMessage("You've purchased a \x0c" .. item.name .. "\x08!", 60*5)
            end
            
            print("Giving " .. item.name .. " to player " .. player:Username())
            player:GiveItem(item.id, false)
        end
    end
end

function TeamRunPlayer:OnUnlockLevel(level)
    Player.OnUnlockLevel(self, level)
    for _, player in ipairs(self:Universe():LuaEntity():FindChildren("Player")) do
        player:UnlockLevel(level)
    end
end