TextAreaElement = class("TextAreaElement", ViewElement)

function TextAreaElement:initialize(x, y, width, height)
    ViewElement.initialize(self, NativeTextAreaElement())
    
    self:SetPosition(x, y)
    self:SetSize(width, height)
    self:SetMargins(5, 5)

    self.TextColor = RGBA(0x88, 0xa8, 0xff, 0xc0)
    self.DrawsBackground = true
end
