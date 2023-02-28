using Godot;


public class SoundMnger : Node
{
    //  资源
    public AudioStream snd_explosion;
    public AudioStream snd_be_killed;

    //  子节点
    AudioStreamPlayer msc_player;
    AudioStreamPlayer snd_player;

    bool msc_loop = true;
    bool snd_loop = false;


    public override void _EnterTree()
    {
        snd_explosion = GD.Load<AudioStream>("res://assets/声音/sound/Explosion Cannon Fire 01.wav");
        snd_be_killed = GD.Load<AudioStream>("res://assets/声音/sound/阵亡.wav");
    }
    public override void _Ready()
    {
        msc_player = GetNode<AudioStreamPlayer>("Music/MusicPlayer");
        snd_player = GetNode<AudioStreamPlayer>("Sound/SoundPlayer");
    }

    // public UpdateSndVolume(){}		//  func update_snd_volume(val, vol_range, is_msc : bool) -> void:  更新音量？


    public void StopAllMsc()
    {
        msc_player.Stop();
    }

    public void StopAllSnd()
    {
        snd_player.Stop();
        snd_loop = false;
    }

    public void PlaySoundRes(AudioStream snd, bool _loop = false)
    {
        if (snd != null)
        {
            snd_player.Stop();
            if (snd_player.Stream != snd) { snd_player.Stream = snd; }
            snd_player.Play();


            if (_loop) { snd_loop = true; }
        }
    }

    void _on_MusicPlayer_finished()
    {
        if (msc_loop) { msc_player.Play(); }
    }

    void _on_SoundPlayer_finished()
    {
        if (snd_loop) { snd_player.Play(); }

    }
}
