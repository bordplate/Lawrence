Course = class('Course', Entity)

function Course:initialize(name)
    -- Load course
    self.checkpoints = {
        {x = 235, y = 167, z = 55.5, rotation = 0.0},
        {x = 264, y = 181, z = 55, rotation = 0.0},
        {x = 310, y = 223, z = 57.5, rotation = 0.0},
        {x = 324, y = 242, z = 62.5, rotation = 0.0},
        {x = 316, y = 268, z = 62.5, rotation = 0.0},
        {x = 271, y = 311, z = 65.5, rotation = 0.0},
        {x = 242.5, y = 303, z = 65.5, rotation = 0.0},
        {x = 175, y = 277, z = 62.5, rotation = 0.0},
        {x = 169, y = 254, z = 60.5, rotation = 0.0},
        {x = 197, y = 254, z = 61.5, rotation = 0.0},
        {x = 251, y = 258, z = 60.5, rotation = 0.0},
        {x = 258, y = 224, z = 55.5, rotation = 0.0},
        {x = 196, y = 225, z = 53.5, rotation = 0.0},
        {x = 191, y = 191, z = 53.5, rotation = 0.0},
        --{x = 232, y = 184, z = 53.5, rotation = 0.0},
    }
    
    self.start = {x = 217, y = 160, z = 56, rotation = 0.0}
    
    self.laps = 1
end

function Course:load(courseData)
    
end