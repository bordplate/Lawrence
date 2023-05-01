using System;
using System.Text;
using Force.Crc32;
using NLua;

namespace Lawrence;

public class Label : Entity {
    private string _text;
    private ushort _x = 0;
    private ushort _y = 0;

    private uint _color = 0;

    private uint _hash = 0;
    
    public Label(LuaTable luaTable, string text = "", ushort x = 0, ushort y = 0, uint color = 0xC0FFA888) : base(luaTable) {
        _text = text;
        _x = x;
        _y = y;
        _color = color;

        ComputeHash();
    }

    public string Text() {
        return _text;
    }

    public ushort X() {
        return _x;
    }

    public ushort Y() {
        return _y;
    }

    public uint Color() {
        return _color;
    }

    /// <summary>
    /// The hash changes when the label is updated, so that we can remove redundant updates to labels that haven't updated
    /// </summary>
    /// <returns></returns>
    public uint Hash() {
        return _hash;
    }

    private void ComputeHash() {
        _hash = Crc32Algorithm.Compute(Encoding.ASCII.GetBytes($"{_x}{_y}{_color}{_text}"));
    }

    public void SetPosition(ushort x, ushort y) {
        _x = x;
        _y = y;
        
        ComputeHash();
    }

    public void SetText(string text) {
        _text = text;
        
        ComputeHash();
    }

    public void SetColor(uint color) {
        _color = color;
        
        ComputeHash();
    }
}