using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundtrackPlayer : MonoBehaviour
{
    #region Singletone
    private static SoundtrackPlayer _default;
    public static SoundtrackPlayer Default => _default;
    #endregion

    private AudioSource _source;
    private bool _playingBoss;

    public void PlaySoundtrack(bool isBoss)
    {
        if (isBoss == _playingBoss && _source.isPlaying)
        {
            return;
        }

        _playingBoss = isBoss;

        SoundHolder.SoundOption sound = SoundHolder.Default.GetRandomSound(isBoss ? "SoundtrackBoss" : "Soundtrack");
        _source.clip = sound.clip;
        _source.volume = sound.volume;
        _source.Play();
    }

    private void Awake()
    {
        _default = this;
        _source = new GameObject("Soundtrack Player").AddComponent<AudioSource>();
        _source.transform.SetParent(transform);
        _source.loop = true;
    }
}
