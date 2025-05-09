using Lawrence.Core;

namespace Lawrence.Game.UI;

public class TextAreaElement: ViewElement {
    public ViewAttribute<string> Text = new(MPUIElementAttribute.Text, "");
    public ViewAttribute<float> TextSize = new(MPUIElementAttribute.TextSize, 1);
    public ViewAttribute<int> LineSpacing = new(MPUIElementAttribute.LineSpacing, 15);
    public ViewAttribute<uint> TextColor = new(MPUIElementAttribute.TextColor, 0x88a8ffc0);
}
