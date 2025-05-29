using Lawrence.Core;

namespace Lawrence.Game.UI;

public class TextAreaElement: ViewElement {
    public ViewAttribute<string> Text = new(MPUIElementAttribute.Text, "");
    public ViewAttribute<float> TextSize = new(MPUIElementAttribute.TextSize, 1);
    public ViewAttribute<int> LineSpacing = new(MPUIElementAttribute.LineSpacing, 15);
    public ViewAttribute<uint> TextColor = new(MPUIElementAttribute.TextColor, 0xc0ffa888);
    public ViewAttribute<bool> HasShadow = new(MPUIElementAttribute.Shadow, false);
}
