using System.Collections.Generic;

namespace Lawrence.Game.UI;

public class View() : Entity(null) {
    private List<ViewElement> _elements = new ();

    private ushort _nextElementId = 0;

    public delegate void OnActivate();
    public event OnActivate Activate;

    public void AddElement(ViewElement element) {
        element.Id = _nextElementId++;
        element.View = this;
        
        _elements.Add(element);
    }
    
    public List<ViewElement> Elements() {
        return _elements;
    }

    public void OnPresent() {
        CallLuaFunction("OnPresent", LuaEntity());
    }

    public void OnControllerInputPresset(Player player, ControllerInput input) {
        CallLuaFunction("OnControllerInputPressed", LuaEntity(), player.LuaEntity(), (uint)input);
    }
}
