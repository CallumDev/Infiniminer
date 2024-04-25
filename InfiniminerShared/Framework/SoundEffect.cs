using LibreLancer.Media;

namespace Infiniminer;

public class SoundEffect
{
    private SoundData data;
    private AudioManager audio;
    public SoundEffect(AudioManager audio, string filename)
    {
        this.audio = audio;
        data = audio.AllocateData();
        data.LoadFile(filename);
    }

    public void Play(float volume)
    {
        var instance = audio.CreateInstance(data, SoundType.Sfx);
        instance.DisposeOnStop = true;
        float db = (1.0f - volume) * -100;
        instance.SetAttenuation(db);
        instance.Play();
    }
}