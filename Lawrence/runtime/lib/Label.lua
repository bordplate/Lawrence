Label = class('Label', Entity)

function Label:initialize(text, x, y, color)
    local labelEntity = Game:NewLabel(self, text, x, y, color)
    
    Entity.initialize(self, labelEntity)
end