using Godot;
using System;

public class SoundMnger : Node
{
//  子节点
	AudioStreamPlayer msc_player;
	AudioStreamPlayer snd_player;

	bool msc_loop = true;
	bool snd_loop = false;

	public override void _Ready()
	{
		msc_player = GetNode<AudioStreamPlayer>("Music/MusicPlayer");
		snd_player = GetNode<AudioStreamPlayer>("Sound/SoundPlayer");

		// msc_player.stream = msc_battle;
		// msc_player.Play();

		//  各种音乐资源

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

	public void PlaySound(int snd_ind, bool _loop = false)
	{
		// 	var snd : AudioStreamSample;
		// match snd_ind :
		// 	Sounds._Attack0_:
		// 		snd = snd_attack0;
		// 	Sounds._Injured_:
		// 		snd = snd_injured;
		// 	Sounds._Defense_:
		// 		snd = snd_defense;


		// if snd != null :
		// 	snd_player.stop();
		// 	snd_player.stream = snd;
		// 	snd_player.play();

		// 	if _loop :
		// 		snd_loop = true;
		// pass
	}

	public void PlaySoundRes(AudioStream snd, bool _loop = false)
	{
		if (snd != null)
		{
			snd_player.Stop();
			snd_player.Stream = snd;
			snd_player.Play();

			if (_loop) snd_loop = true;
		}
	}

	void _on_MusicPlayer_finished()
	{
		if (msc_loop)
			msc_player.Play();
	}

	void _on_SoundPlayer_finished()
	{
		if (snd_loop)
			snd_player.Play();

	}
}
