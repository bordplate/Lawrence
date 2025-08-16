HoverboardLadyMoby = class("HoverboardLadyMoby", HybridMoby)

function HoverboardLadyMoby:initialize(level, uid, universe)
    HybridMoby.initialize(self, level, uid)

    self:MonitorPVar(4, 2, false)
    self.universe = universe

    self.state = 0
end

function HoverboardLadyMoby:OnPVarChange(player, offset, oldValue, newValue)
    if newValue == 4 then
        self.universe:OnPlayerGetItem(nil, 0x30) -- Zoomerator
    end
end

function HoverboardLadyMoby:Triangle()
end

function HoverboardLadyMoby:closeToPlayer(player)
end

function HoverboardLadyMoby:ToastMessage(player)
end 