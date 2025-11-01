using Core;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.AddressableAssets;
using System;

public enum SoundType
{
    BGM,
    SFX,
}

public class SoundManager : Singleton<SoundManager>, IManager
{
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private string[] _audioClipNames; // 미리 로딩할 클립이름들

    private Dictionary<string, AudioClip> _clipDict = new();

    private Stack<TemporarySoundPlayer> _soundPlayerPool = new();
    private List<TemporarySoundPlayer> _loopSoundPlayers = new();

    protected override void AfterAwake()
    {
        Initialize();
    }

    public async void Initialize()
    {
        if (_audioMixer == null)
        {
            _audioMixer = await Addressables.LoadAssetAsync<AudioMixer>("Audio/AudioMixer").ToUniTask();
        }

        AddClipsToClipDict(_audioClipNames);

        PlaySound2D("village", isLoop: true, type: SoundType.BGM);
    }

    public void AddClipsToClipDict(string[] names)
    {
        foreach (var name in _audioClipNames)
        {
            GetClip(name);
        }
    }

    public AudioClip GetClip(string name)
    {
        if (_clipDict.TryGetValue(name, out var clip))
        {
            return clip;
        }

        clip = SafeAddressableLoader.LoadSync<AudioClip>("AudioClips/" + name);

        if (clip != null)
        {
            _clipDict[name] = clip;
        }

        return clip;
    }

    private TemporarySoundPlayer PopSoundPlayer()
    {
        TemporarySoundPlayer soundPlayer;

        if (_soundPlayerPool.Count > 0) return _soundPlayerPool.Pop();

        GameObject obj = new("TemporarySoundPlayer");
        soundPlayer = obj.AddComponent<TemporarySoundPlayer>();

        return soundPlayer;
    }

    private void AddToLoopList(TemporarySoundPlayer soundPlayer)
    {
        _loopSoundPlayers.Add(soundPlayer);
    }

    public void PlaySound2D(string clipName, float delay = 0f, bool isLoop = false, SoundType type = SoundType.SFX, Action clipEndEvent = null)
    {
        var soundPlayer = PopSoundPlayer();

        soundPlayer.InitSoundClip(GetClip(clipName), clipEndEvent);

        if (isLoop) { AddToLoopList(soundPlayer); }
        
        soundPlayer.Play(_audioMixer.FindMatchingGroups(type.ToString())[0], delay, isLoop).Forget();
    }

    public void PlaySound3D(string clipName, Transform audioTarget, float delay = 0f, bool isLoop = false, SoundType type = SoundType.SFX, bool attachToTarget = true, float minDistance = 0.0f, float maxDistance = 50.0f, AudioRolloffMode rolloffMode = AudioRolloffMode.Linear, Action clipEndEvent = null)
    {
        var soundPlayer = PopSoundPlayer();

        if (attachToTarget) { soundPlayer.transform.parent = audioTarget; }

        soundPlayer.InitSoundClip(GetClip(clipName), clipEndEvent);
        soundPlayer.Init3DProperty(minDistance, maxDistance, rolloffMode);

        if (isLoop) { AddToLoopList(soundPlayer); }

        if (attachToTarget)
        {
            // 부모로 붙이고 로컬 0
            soundPlayer.transform.SetParent(audioTarget, worldPositionStays: false);
            soundPlayer.transform.localPosition = Vector3.zero;
            soundPlayer.transform.localRotation = Quaternion.identity;
        }
        else
        {
            // 부모 해제하고 월드 위치/회전만 복사
            soundPlayer.transform.SetParent(null);
            soundPlayer.transform.position = audioTarget.position;
            soundPlayer.transform.rotation = audioTarget.rotation;
        }

        soundPlayer.Play(_audioMixer.FindMatchingGroups(type.ToString())[0], delay, isLoop).Forget();
    }

    public void StopLoopSound(string clipName, bool stopInstant = true)
    {
        foreach (TemporarySoundPlayer audioPlayer in _loopSoundPlayers)
        {
            if (audioPlayer.ClipName == clipName)
            {
                _loopSoundPlayers.Remove(audioPlayer);

                if (stopInstant)
                {
                    audioPlayer.Stop();
                }
                else
                {
                    audioPlayer.StopLoop();
                }

                return;
            }
        }

        Debug.LogWarning(clipName + "을 찾을 수 없습니다.");
    }

    public void PushSoundPlayer(TemporarySoundPlayer soundPlayer)
    {
        if (!_soundPlayerPool.Contains(soundPlayer))
        {
            soundPlayer.transform.parent = transform;
            _soundPlayerPool.Push(soundPlayer);
        }
    }

    public void ClearCache()
    {
        foreach (var clip in _clipDict.Values)
        {
            Addressables.Release(clip);
        }

        _clipDict.Clear();
    }

    public void InitVolumes(float bgm, float sfx)
    {
        SetVolume(SoundType.BGM, bgm);
        SetVolume(SoundType.SFX, sfx);
    }

    public void SetVolume(SoundType type, float value)
    {
        _audioMixer.SetFloat(type.ToString(), value);
    }
}
