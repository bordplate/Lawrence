using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Lawrence.Core;

namespace Lawrence.Game.UI;

public class ViewElement: Entity {
    public ushort Id = 0;
    public View? View;

    public ViewAttribute<Vector2> Position = new(MPUIElementAttribute.Position, new (0, 0));
    public ViewAttribute<Vector2> Size = new(MPUIElementAttribute.Size, new (0, 0));
    public ViewAttribute<Vector2> Margins = new(MPUIElementAttribute.Margins, new (0, 0));

    public ViewAttribute<bool> Visible = new(MPUIElementAttribute.Visible, true);
    
    public ViewAttribute<uint> States = new(MPUIElementAttribute.States, 0xff);
    public ViewAttribute<bool> DrawsBackground = new(MPUIElementAttribute.DrawsBackground, false);
    
    public void SetSize(float width, float height) {
        Size.Value = new Vector2(width, height);
    }
    
    public void SetPosition(float x, float y) {
        Position.Value = new Vector2(x, y);
    }
    
    public void SetMargins(float x, float y) {
        Margins.Value = new Vector2(x, y);
    }
    
    public List<IViewAttribute> GetAttributes() {
        return GetType().GetFields()
            .Where(f => f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(ViewAttribute<>))
            .Select(f => (IViewAttribute)f.GetValue(this)!)
            .ToList();
    }
}
