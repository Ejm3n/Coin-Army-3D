using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "Scriptables/SoundHolder")]
public class SoundHolder : DataHolder
{
    #region Singletone
    private static SoundHolder _default;
    public static SoundHolder Default => _default;
    #endregion

    [Serializable] public class SoundOption 
    {
        public AudioClip clip;
        public float volume = 1f;
    }
    [Serializable] public class SoundPack 
    {
        public string key = "";
        public List<SoundOption> sounds;
    }

    public List<SoundPack> soundPacks;


    public override void Init()
    { 
        _default = this;
    }

    /// <summary>
    /// Спавнит GameObject c AudioSource, помещает в него рандомный клип из набора с соответствующим названем, задает настройки громкости.
    /// Объект автоматически удаляется после проигрывания клипа.
    /// </summary>
    public AudioSource PlayFromSoundPack(string packName, bool isLoop = false, float delay = 0f, bool allowPitchShift = true)
    {
        SoundPack pack = soundPacks.Find((p) => p.key == packName);

        if (pack == null || pack.sounds.Count <= 0) return null;
        SoundOption sound = pack.sounds[UnityEngine.Random.Range(0, pack.sounds.Count)];
        if (!sound.clip) return null;

        return SpawnSoundSource(sound.clip, sound.volume, isLoop, delay, allowPitchShift);
    }

    /// <summary>
    /// Возвращает рандомный звук с настройками громкости из соответствующего набора.
    /// </summary>
    public SoundOption GetRandomSound(string key) 
    {
        SoundPack pack = soundPacks.Find((p) => p.key == key);
        if (pack == null || pack.sounds.Count <= 0) return null;
        return pack.sounds[UnityEngine.Random.Range(0, pack.sounds.Count)];
    }

    private AudioSource SpawnSoundSource(AudioClip clip, float volume = 1f, bool isLoop = false, float delay = 0f, bool allowPitchShift = true)
    {
        AudioSource source = new GameObject("AudioPlayer").AddComponent<AudioSource>();
        source.transform.SetParent(Camera.main.transform);
        source.transform.localPosition = Vector3.zero;
        source.clip = clip;
        if (allowPitchShift)
        {
            source.pitch += UnityEngine.Random.Range(-0.1f, 0.1f);
        }
        source.volume = volume;
        source.loop = isLoop;
        if (delay > 0f)
        {
            source.PlayDelayed(delay);
        }
        else
        {
            source.Play();
        }
        if (!isLoop) Destroy(source.gameObject, delay + clip.length + 0.5f);
        return source;
    }
}
