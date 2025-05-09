using Lawrence.Core;

namespace Lawrence.Game.UI;

public class TextElement : ViewElement {
    public ViewAttribute<string> Text = new(MPUIElementAttribute.Text, "<No Text>");
    public ViewAttribute<uint> TextColor = new(MPUIElementAttribute.TextColor,  0xffa8ff88);
    public ViewAttribute<bool> HasShadow = new(MPUIElementAttribute.Shadow, true);
}
