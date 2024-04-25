using System.Collections.Generic;
using System.IO;
using LibreLancer;
using LibreLancer.Media;
using LibreLancer.Graphics;
using SharpDX.Multimedia;

namespace Infiniminer;

public class ContentManager
{
    private string basePath;

    private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
    private Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();
    public AudioManager AudioManager;
    RenderContext context;

    public ContentManager(RenderContext context)
    {
        basePath = Path.Combine(Platform.GetBasePath(), "Content");
        this.context = context;
    }

    public string GetPath(string file) => Path.Combine(basePath, file);
    
    public void AddFontFile(string file)
    {
        Platform.AddTtfFile(Path.Combine(basePath, file), File.ReadAllBytes(Path.Combine(basePath, file)));
    }
    
    public SoundEffect LoadSound(string filename)
    {
        if (!soundEffects.TryGetValue(filename, out var sound))
        {
            sound = new SoundEffect(AudioManager, Path.Combine(basePath, filename));
            soundEffects.Add(filename, sound);
        }
        return sound;
    }

    public Texture2D LoadTexture(string texture)
    {
        if (!texture.EndsWith(".png"))
            texture = texture + ".png";
        if (!textures.TryGetValue(texture, out var tex))
        {
            using var stream = File.OpenRead(Path.Combine(basePath, texture));
            tex = (Texture2D)LibreLancer.ImageLib.Generic.TextureFromStream(context, stream, false);
            textures.Add(texture, tex);
        }
        return tex;
    }
}