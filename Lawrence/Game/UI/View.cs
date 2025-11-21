using System.Collections.Generic;
using NLua;

namespace Lawrence.Game.UI;

public class View() : Entity(null) {
    private List<ViewElement> _elements = new ();

    private static ushort _nextElementId = 0;  // Static field to keep track of the next element ID, will overflow eventually, 
                                               // but most likely won't cause an issue in practice.
                                               // Technically, it's possible that an early player's view elements could be
                                               // overwritten by another player if they somehow despawn and respawn a lot of
                                               // UI elements.
                                               // A better approach could be to use the upper 4 bits of the ID as a "view ID"
                                               // unique for each player, which would allow for 16 different views at a time for each player. 
    
    public delegate void OnActivate();

    public delegate void OnClose();

    public delegate void OnElementAdded(ViewElement element);
    public delegate void OnElementRemoved(ViewElement element);
    public event OnActivate? Activate;
    public event OnClose? Close;
    public event OnElementAdded? ElementAdded;
    public event OnElementRemoved? ElementRemoved;
    

    public void AddElement(ViewElement element) {
        element.Id = _nextElementId++;
        element.View = this;
        
        _elements.Add(element);
        
        ElementAdded?.Invoke(element);
    }
    
    public void RemoveElement(ViewElement element) {
        if (_elements.Remove(element)) {
            ElementRemoved?.Invoke(element);
        }
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

    public void CloseView() {
        Close?.Invoke();
        Delete();
    }

    public override void Delete() {
        foreach (var element in _elements) {
            element.View = null;
            element.Delete();
        }
        
        base.Delete();
    }
}
