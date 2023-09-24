local json = require ("dkjson")


Course = class('Course', Entity)

function Course:initialize(filename)
    -- Initialize the course data here as empty
    self.checkpoints = {}
    self.start = {}
    self.laps = 0
    
    self:load("mods/race/courses/" .. filename)
end

function Course:load(filePath)
    print("Loading course from " .. filePath)
    
    -- Read JSON file and parse it
    local file = io.open(filePath, "r")
    local content = file:read("*a")
    file:close()

    local courseData = json.decode(content)

    -- Load the course data from JSON
    self.checkpoints = courseData.checkpoints
    self.start = courseData.start
    self.laps = courseData.laps
    self.name = courseData.name
    self.planet = courseData.planet
    self.delete = courseData.delete
    self.deleteUIDs = courseData.deleteUIDs
    
    print("Loaded course: " .. self.name)
end