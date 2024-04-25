using System.Numerics;
using LibreLancer;
using LibreLancer.Graphics;
using LibreLancer.Graphics.Text;

namespace Infiniminer;

public record FontDescription(string Name, int Size);

public static class RendererExtensions
{
    public static void Draw(this Renderer2D renderer, Texture2D tex, Vector2 pos, Color4 tint)
    {
        var src = new Rectangle(0, 0, tex.Width, tex.Height);
        var dst = new Rectangle((int) pos.X, (int) pos.Y, tex.Width, tex.Height);
        renderer.Draw(tex, src, dst, tint);
    }
    
    public static void DrawString(this Renderer2D renderer, FontDescription fnt, string text, Vector2 vec, Color4 color, OptionalColor shadow = default)
    {
        renderer.DrawStringBaseline(fnt.Name, fnt.Size, text, vec.X, vec.Y, color, false, shadow);
    }

    public static Vector2 MeasureString(this Renderer2D renderer, FontDescription fnt, string text)
    {
        var sz = renderer.MeasureString(fnt.Name, fnt.Size, text);
        return new Vector2(sz.X, sz.Y);
    }
}


public static class Fonts
{
    public static FontDescription UiFont = new FontDescription("VT323", 22);
    public static FontDescription NameFont = new FontDescription("VT323", 18);
    public static FontDescription RadarFont = new FontDescription("VT323", 16);
}