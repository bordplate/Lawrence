using System.Collections.Generic;
using NLua;

namespace Lawrence.Game.UI;

public class View(LuaTable playerTable) : Entity(null) {
    private List<ViewElement> _elements = new ();

    private ushort _nextElementId = 0;

    public LuaTable PlayerTable { get; private set; } = playerTable;
    
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

    public void OnControllerInputPresset(ControllerInput input) {
        CallLuaFunction("OnControllerInputPressed", LuaEntity(), (uint)input);
    }
}
