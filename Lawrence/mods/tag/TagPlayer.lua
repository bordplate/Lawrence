---
--- Created by bordplate.
--- DateTime: 21/05/2023 16:08
---

TagPlayer = class('TagPlayer', Player)

function TagPlayer:Made()
    self:RemoveAllLabels()

    self.runnerLabel = Label:new("Runner", 60, 60, 0xffff0000)
    self.ticksLabel = Label:new(self:Ticks() .. " player ticks", 400, 400, 0xC0FFA888)

    self:AddLabel(self.runnerLabel)
    self:AddLabel(self.ticksLabel)
end

function TagPlayer:OnTick()
    self.ticksLabel:SetText(self:Ticks() .. " player ticks")
end

function TagPlayer:OnCollision()

end
 