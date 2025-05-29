using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Lawrence.Core;

namespace Lawrence.Game.UI;

public interface IViewAttribute {
    public bool Dirty { get; set; }
    public MPUIElementAttribute ElementAttribute { get; }
    
    public MPPacket? GetPacket();
}

public class ViewAttribute<T> : IViewAttribute {
    private T _value;
    private bool _dirty;

    public bool Dirty {
        get {
            if (this is ViewAttribute<List<ListMenuItem>> listMenuItemAttribute) {
                return listMenuItemAttribute.Value.Any(menuItem => menuItem.Dirty);
            }
            return _dirty;
        }
        set => _dirty = value;
    }

    public MPUIElementAttribute ElementAttribute { get; }

    public T Value
    {
        get => _value;
        set
        {
            _value = value;
            Dirty = true;
        }
    }

    public ViewAttribute(MPUIElementAttribute elementAttribute, T initialValue) {
        ElementAttribute = elementAttribute;
        _value = initialValue;

        Dirty = true;
    }

    public override string ToString() => $"Value: {_value}, Dirty: {Dirty}";

    public void Set(T value) {
        Value = value;
    }

    public MPPacket? GetPacket() {
        var packet = new MPPacketData();
        
        switch (this) {
            case ViewAttribute<Vector2> vector2Attribute: {
                var x = BitConverter.GetBytes((ushort)vector2Attribute.Value.X).Reverse();
                var y = BitConverter.GetBytes((ushort)vector2Attribute.Value.Y).Reverse();
                
                packet.Write(x.ToArray());
                packet.Write(y.ToArray());
                break;
            }
            case ViewAttribute<Vector3?> vector3Attribute: {
                if (vector3Attribute.Value == null) {
                    return null; // No packet if the value is null
                }
                
                var x = BitConverter.GetBytes(vector3Attribute.Value.Value.X).Reverse();
                var y = BitConverter.GetBytes(vector3Attribute.Value.Value.Y).Reverse();
                var z = BitConverter.GetBytes(vector3Attribute.Value.Value.Z).Reverse();
                
                packet.Write(x.ToArray());
                packet.Write(y.ToArray());
                packet.Write(z.ToArray());
                
                break;
            }
            case ViewAttribute<bool> boolAttribute: {
                var value = BitConverter.GetBytes(boolAttribute.Value ? (int) 1 : 0);
                
                packet.Write(value.ToArray());
                break;
            }
            case ViewAttribute<uint> uintAttribute: {
                var value = BitConverter.GetBytes(uintAttribute.Value).Reverse();
                
                packet.Write(value.ToArray());
                break;
            }
            case ViewAttribute<int> intAttribute: {
                var value = BitConverter.GetBytes(intAttribute.Value).Reverse();
                
                packet.Write(value.ToArray());
                break;
            }
            case ViewAttribute<float> floatAttribute: {
                var value = BitConverter.GetBytes(floatAttribute.Value).Reverse();
                
                packet.Write(value.ToArray());
                break;
            }
            case ViewAttribute<string> stringAttribute: {
                var value = System.Text.Encoding.UTF8.GetBytes(stringAttribute.Value);
                
                packet.Write(value);
                packet.Write([0x0]);
                break;
            }
            case ViewAttribute<List<ListMenuItem>> listMenuItemAttribute: {
                byte i = 0;
                foreach (var menuItem in listMenuItemAttribute.Value) {
                    if (!menuItem.Dirty) {
                        i++;
                        continue;
                    }

                    menuItem.Dirty = false;
                    
                    var title = System.Text.Encoding.UTF8.GetBytes(menuItem.Title);
                    var details = System.Text.Encoding.UTF8.GetBytes(menuItem.Details);
                    var accessory = System.Text.Encoding.UTF8.GetBytes(menuItem.Accessory);
                    
                    packet.Write([i]);
                    packet.Write([menuItem.ShouldDelete ? (byte)1 : (byte)0]);
                    packet.Write(title);
                    packet.Write([0x0]);
                    packet.Write(details);
                    packet.Write([0x0]);
                    packet.Write(accessory);
                    packet.Write([0x0]);

                    if (!menuItem.ShouldDelete) {
                        i++;
                    }
                }
                
                listMenuItemAttribute.Value.RemoveAll(menuItem => menuItem.ShouldDelete);
                
                break;
            }
            default:
                throw new NotImplementedException($"Unknown view attribute type: {this.GetType()}. If you want to use " +
                                                  $"this type, you need to implement it in the switch statement. " +
                                                  $"Remember to account for endianness.");
        }
        
        return packet;
    }
}
