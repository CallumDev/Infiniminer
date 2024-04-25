using System.Numerics;
using LibreLancer;
using LibreLancer.Graphics;

namespace Infiniminer;

public class Effect
{
    public static RenderContext Context;
    
    public Shader Shader;

    private int worldLocation;
    private int timeLocation;
    private int lodColorLocation;
    private int colorLocation;
    private Matrix4x4 _world;

    public Matrix4x4 World
    {
        get { return _world;  }
        set {
            _world = value;
            Shader.SetMatrix(worldLocation, ref _world);
        }
    }

    private float _time;

    public float Time
    {
        get { return _time; }
        set {
            _time = value;
            Shader.SetFloat(timeLocation, _time);
        }
    }

    private Color4 _lodColor;
    public Color4 LODColor
    {
        get => _lodColor;
        set
        {
            _lodColor = value;
            Shader.SetColor4(lodColorLocation, _lodColor);
        }
    }
    
    private Color4 _color;
    public Color4 Color
    {
        get => _color;
        set
        {
            _color = value;
            Shader.SetColor4(colorLocation, _color);
        }
    }

    private Effect(Shader sh)
    {
        sh.SetInteger(sh.GetLocation("Texture"), 0);
        worldLocation = sh.GetLocation("World");
        timeLocation = sh.GetLocation("Time");
        lodColorLocation = sh.GetLocation("LODColor");
        colorLocation = sh.GetLocation("Color");
        Shader = sh;
    }

    public static Effect Compile(string vertex, string fragment)
    {
        vertex = "#version 150\n" + vertex;
        fragment = "#version 150\n" + fragment;
        var sh = new Shader(Context, vertex, fragment);
        return new Effect(sh);
    }

    public static void Log(string text) => FLLog.Info("Shaders", text);
}