TextElement = class("TextElement", ViewElement)

function TextElement:initialize(x, y, text)
    ViewElement.initialize(self, NativeTextElement())
    
    self:SetPosition(x, y)
    
    self.TextColor = RGBA(0x88, 0xa8, 0xff, 0xc0)
    self.Text = text
end
