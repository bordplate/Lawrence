GoldWeaponCaseMoby = class("GoldWeaponCaseMoby", Moby)

function GoldWeaponCaseMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(304)

    self.scale = 0.12

    self.bolt_cost = 0
    self.gold_bolt_cost = 0
    
    self.item_name = "Suck Cannon"
    
    self.item_id = 400 -- placeholder value must be changed (400 + gold weapon offset)

    self.disabled = false
end

function GoldWeaponCaseMoby:closeToPlayer(player)
    if self:Level():GetName() == player:Level():GetName() and
            self.x - 2.5 <= player.x and player.x <= self.x + 2.5 and
            self.y - 2.5 <= player.y and player.y <= self.y + 2.5 and
            self.z - 2.5 <= player.z and player.z <= self.z + 2.5 then
        return true
    end
    return false
end

function GoldWeaponCaseMoby:ToastMessage(player)
    if not self.disabled and self:closeToPlayer(player) then
        universe = player.lobby.universe
        if universe.totalBolts >= self.bolt_cost and universe.num_received_gold_bolts - universe.used_gold_bolts >= self.gold_bolt_cost then
            player:ToastMessage(string.format("\x12 Buy \x0c%s\x08 for %d bolts and %d gold bolts", self.item_name, self.bolt_cost, self.gold_bolt_cost), 1)
        else
            player:ToastMessage(string.format("\x0c%s\x08: %d bolts %d gold bolts (%d gold bolts)", self.item_name, self.bolt_cost, self.gold_bolt_cost, universe.num_received_gold_bolts - universe.used_gold_bolts), 1)
        end
    end
end

function GoldWeaponCaseMoby:Triangle(player, universe)
    if not self.disabled and self:closeToPlayer(player) and universe.totalBolts >= self.bolt_cost and universe.num_received_gold_bolts - universe.used_gold_bolts >= self.gold_bolt_cost then
        universe:GiveBolts(-self.bolt_cost)
        universe:OnPlayerGetItem(player, self.item_id)
    end
end

function GoldWeaponCaseMoby:Disable()
    self.disabled = true
    if self.weapon then
        self.weapon:Delete()
        self.weapon = nil
    end
end 

function GoldWeaponCaseMoby:AttachWeapon(weapon_oclass, level)
    self.weapon = level:SpawnMoby(weapon_oclass)
    self.weapon:SetOClass(weapon_oclass)
    self.weapon.scale = 0.12
    self.weapon:SetPosition(self.x, self.y, self.z + 0.5)
end 