---
--- Created by bordplate.
--- DateTime: 21/05/2023 16:08
---

HunterPlayer = class('HunterPlayer', Player)

function HunterPlayer:Made()
    self:RemoveAllLabels()

    self.hunterLabel = Label:new("Hunter", 60, 60, 0xff0000ff)
    self.ticksLabel = Label:new(self:Ticks() .. " player ticks", 400, 400, 0xC0FFA888)

    self:AddLabel(self.hunterLabel)
    self:AddLabel(self.ticksLabel)
end

function HunterPlayer:OnTick()
    self.ticksLabel:SetText(self:Ticks() .. " player ticks")
end

function HunterPlayer:OnAttack(moby)
    if moby:Is(TagPlayer) then
        moby:Make(HunterPlayer)
        self:Make(TagPlayer)
    end
end

function HunterPlayer:OnCollision()

end
