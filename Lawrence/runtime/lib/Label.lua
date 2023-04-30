Manager = class('Manager', Entity)

function Manager:initialize()
    Entity.initialize(self)

    self.Active = false
end