using System.Collections.Generic;
using Lawrence.Core;
using NLua;

namespace Lawrence.Game.UI;

public class ListMenuItem {
    private string _title = "";
    public string Title {
        get => _title;
        set {
            Dirty = true;
            _title = value;
        }
    }

    private string _details = "";
    public string Details {
        get => _details;
        set {
            Dirty = true;
            _details = value;
        }
    }
    
    private string _accessory = "";
    public string Accessory {
        get => _accessory;
        set {
            Dirty = true;
            _accessory = value;
        }
    }

    public bool ShouldDelete = false;

    public void Remove() {
        Dirty = true;
        ShouldDelete = true;
    }

    public bool Dirty = true;
}

public class ListMenuElement: ViewElement {
    public ViewAttribute<float> TitleSize = new(MPUIElementAttribute.TitleTextSize, 1);
    public ViewAttribute<float> DetailsSize = new(MPUIElementAttribute.DetailsTextSize, 0.7f);
    
    public ViewAttribute<uint> DefaultColor = new(MPUIElementAttribute.MenuDefaultColor, 0x88a8ffff);
    public ViewAttribute<uint> SelectedColor = new(MPUIElementAttribute.MenuSelectedColor, 0x88a800ff);
    
    public ViewAttribute<int> ElementSpacing = new(MPUIElementAttribute.ElementSpacing, 20);
    public ViewAttribute<int> SelectedItem = new(MPUIElementAttribute.MenuSelectedItem, 0);
    
    public ViewAttribute<List<ListMenuItem>> Items = new(MPUIElementAttribute.MenuItems, new ());

    public delegate void MakeFocused();
    public MakeFocused? MakeFocusedDelegate;
    
    public void AddItem(string title, string details = "", string accessory = "") {
        Items.Value.Add(new ListMenuItem { Title = title, Details = details, Accessory = accessory});
    }
    
    public void RemoveItem(int index) {
        Items.Value[index].Remove();
    }
    
    public ListMenuItem GetItem(int index) {
        return Items.Value[index];
    }
    
    public LuaTable GetItems() {
        var lua = LuaEntity();
        var table = Game.Shared().State().DoString("return {}")[0] as LuaTable;
        for (var i = 0; i < Items.Value.Count; i++) {
            var item = Items.Value[i];

            table[i + 1] = item;
        }
        
        return table;
    }

    public void Focus() {
        MakeFocusedDelegate?.Invoke();
    }
    
    public void OnItemActivated(uint index) {
        CallLuaFunction("OnItemActivated", LuaEntity(), index);
    }
    
    public void OnItemSelected(uint index) {
        SelectedItem.Value = (int)index;
        SelectedItem.Dirty = false;
        
        CallLuaFunction("OnItemSelected", LuaEntity(), index);
    }
}
