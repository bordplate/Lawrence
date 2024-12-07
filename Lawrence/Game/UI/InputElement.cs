using Lawrence.Core;

namespace Lawrence.Game.UI;

public class InputElement : ViewElement {
    public delegate void OnActivate();
    public OnActivate? ActivateDelegate;
    
    public ViewAttribute<string> Prompt = new(MPUIElementAttribute.InputPrompt, "Enter text");

    public void Activate() {
        ActivateDelegate?.Invoke();
    }

    public void OnInputCallback(Player player, string text) {
        CallLuaFunction("OnInputCallback", LuaEntity(), player.LuaEntity(), text);
    }
}
