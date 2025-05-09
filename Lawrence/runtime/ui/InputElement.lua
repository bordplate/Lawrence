InputElement = class("InputElement", ViewElement)

function InputElement:initialize()
    ViewElement.initialize(self, NativeInputElement())
    
    self.InputCallback = null
end 

function InputElement:OnInputCallback(player, input)
    if self.InputCallback then
        self.InputCallback(player, input)
    end
end
