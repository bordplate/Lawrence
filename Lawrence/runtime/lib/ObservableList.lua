ObservableList = class("ObservableList")

ObservableList.ADDED = 1
ObservableList.REMOVED = 2
ObservableList.CLEARED = 3

function ObservableList:initialize(list)
    self.list = list
    
    self.observers = {}
end

function ObservableList:AddObserver(observerCallback)
    table.insert(self.observers, observerCallback)
end

function ObservableList:RemoveObserver(observerCallback)
    for i, v in ipairs(self.observers) do
        if v == observerCallback then
            table.remove(self.observers, i)
            break
        end
    end
end

function ObservableList:NotifyObservers(action, item)
    for i, v in ipairs(self.observers) do
        v(self.list, action, item)
    end
end

function ObservableList:Add(item)
    table.insert(self.list, item)
    self:NotifyObservers(ObservableList.ADDED, item)
end

function ObservableList:Remove(item)
    for i, v in ipairs(self.list) do
        if v == item then
            table.remove(self.list, i)
            break
        end
    end
    self:NotifyObservers(ObservableList.REMOVED, item)
end

function ObservableList:Clear()
    self.list = {}
    self:NotifyObservers(ObservableList.CLEARED, null)
end

function ObservableList:Count()
    return #self.list
end

function ObservableList:Get(index)
    return self.list[index]
end

function ObservableList:__len()
    return #self.list
end

function ObservableList:__index(key)
    if type(key) == "number" then
        return self.list[key]
    else
        return ObservableList[key]
    end
end
