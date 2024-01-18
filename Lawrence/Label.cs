using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;
using Force.Crc32;
using NLua;

namespace Lawrence;

public class Label : Entity {
    private string _text;
    private ushort _x = 0;
    private ushort _y = 0;

    private uint _color = 0;

    private uint _states = 0;
    public bool HasChanged { get; private set; }

    public Label(LuaTable luaTable, string text = "", ushort x = 0, ushort y = 0, uint color = 0xC0FFA888) :
        base(luaTable) {
        _text = text;
        _x = x;
        _y = y;
        _color = color;

        HasChanged = true;

        Game.Shared().NotificationCenter().Subscribe<PreTickNotification>(OnPreTick);
    }

    ~Label() {
        Game.Shared().NotificationCenter().Unsubscribe<PreTickNotification>(OnPreTick);
    }

    public void OnPreTick(PreTickNotification notification) {
        HasChanged = false;
    }

    public void ForceSetChanged() {
        HasChanged = true;
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

    public void SetFlag(int state) {
        _states |= (uint)1 << state;
    }
    public uint States() {
        return _states;
    }

    public void SetPosition(ushort x, ushort y) {
        HasChanged = (_x != x || _y != y);

        _x = x;
        _y = y;
    }

    public void SetText(string text) {
        HasChanged = !_text.Equals(text);

        _text = text;
    }

    public void SetColor(uint color) {
        HasChanged = _color != color;

        _color = color;
    }
}