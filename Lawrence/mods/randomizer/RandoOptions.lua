RandoOptions = class("RandoOptions")

function RandoOptions:initialize(options)
    self.options = options
    
    self.observerCallbacks = {}
end

function RandoOptions:AddObserver(callback)
    table.insert(self.observerCallbacks, callback)
end

function RandoOptions:__index(key)
    local obj = self.options[key]
    
    local this = self

    obj.set = function(self, value)
        for i, callback in ipairs(this.observerCallbacks) do
            callback(this.options, key, value)
        end

        self.value = value
    end
    
    setmetatable(obj, {
        __call = function(self, view, item)
            return obj.handler(obj, view, item)
        end,
        __newindex = function(self, key, value)
            if key == "value" then
                self.set(value)
            end
        end
    })
    
    return obj
end

--function RandoOptions:__newindex(key, value)
--    
--    self.options[key].value = value
--end
