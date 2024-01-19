Label = class('Label', Entity)

function Label:initialize(text, x, y, color, states)
    states = states or {GameState.PlayerControl} -- empty list
    local labelEntity = Game:NewLabel(self, text, x, y, color)
    
    for i, state in ipairs(states) do
        labelEntity:SetFlag(state)
    end

    Entity.initialize(self, labelEntity)
end

GameState = {
    PlayerControl = 0,
    Movie = 1,
    CutScene = 2,
    Menu = 3,
    ExitRace = 4,
    Gadgetron = 5,
    PlanetLoading = 6,
    CinematicMaybe = 7
}