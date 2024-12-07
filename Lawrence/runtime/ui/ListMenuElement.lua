ListMenuElement = class("ListMenuElement", ViewElement)

function ListMenuElement:initialize(x, y, width, height)
    ViewElement.initialize(self, NativeListMenuElement())
    
    self:SetPosition(x, y)
    self:SetSize(width, height)
    self:SetMargins(5, 5)
    
    self.DrawsBackground = true
    
    self.DefaultColor = RGBA(0x88, 0xa8, 0xff, 0xc0)
    self.SelectedColor = RGBA(0x88, 0xa8, 0x0, 0xc0)
    
    self.ItemActivated = null
    self.ItemSelected = null
end

function ListMenuElement:OnItemActivated(index)
    if (self.ItemActivated ~= null) then
        self.ItemActivated(index)
    end
end

function ListMenuElement:OnItemSelected(index)
    if (self.ItemSelected ~= null) then
        self.ItemSelected(index)
    end
end
