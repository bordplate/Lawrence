Label = class('Label', Entity)

function Label:initialize(text, x, y, color, state)
    state = state or 0
    local labelEntity = Game:NewLabel(self, text, x, y, color, state)
    
    Entity.initialize(self, labelEntity)
end