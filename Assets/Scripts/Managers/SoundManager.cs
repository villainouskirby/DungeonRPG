using Core;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.AddressableAssets;

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
        if (_audioMixer != null) return;

        _audioMixer = await Addressables.LoadAssetAsync<AudioMixer>("Audio/AudioMixer").ToUniTask();

        AddClipsToClipDict(_audioClipNames);
    }

    public async void AddClipsToClipDict(string[] names)
    {
        foreach (var name in _audioClipNames)
        {
            await GetClip(name);
        }
    }

    public async UniTask<AudioClip> GetClip(string name)
    {
        if (_clipDict.TryGetValue(name, out var clip))
        {
            return clip;
        }

        clip = await Addressables.LoadAssetAsync<AudioClip>("AudioClips/" + name).ToUniTask();

        if (clip != null)
        {
            _clipDict[name] = clip;
        }

        return clip;
    }

    private TemporarySoundPlayer PopSoundPlayer()
    {
        if (_soundPlayerPool.Count > 0) return _soundPlayerPool.Pop();

        GameObject obj = new("TemporarySoundPlayer");
        TemporarySoundPlayer soundPlayer = obj.AddComponent<TemporarySoundPlayer>();

        return soundPlayer;
    }

    private void AddToLoopList(TemporarySoundPlayer soundPlayer)
    {
        _loopSoundPlayers.Add(soundPlayer);
    }

    public async void PlaySound2D(string clipName, float delay = 0f, bool isLoop = false, SoundType type = SoundType.SFX)
    {
        var soundPlayer = PopSoundPlayer();

        if (isLoop) { AddToLoopList(soundPlayer); }

        soundPlayer.InitSound2D(await GetClip(clipName));
        soundPlayer.Play(_audioMixer.FindMatchingGroups(type.ToString())[0], delay, isLoop).Forget();
    }

    public async void PlaySound3D(string clipName, Transform audioTarget, float delay = 0f, bool isLoop = false, SoundType type = SoundType.SFX, bool attachToTarget = true, float minDistance = 0.0f, float maxDistance = 50.0f)
    {
        var soundPlayer = PopSoundPlayer();

        if (attachToTarget) { soundPlayer.transform.parent = audioTarget; }

        if (isLoop) { AddToLoopList(soundPlayer); }

        soundPlayer.InitSound3D(await GetClip(clipName), minDistance, maxDistance);
        soundPlayer.Play(_audioMixer.FindMatchingGroups(type.ToString())[0], delay, isLoop).Forget();
    }

    public void StopLoopSound(string clipName)
    {
        foreach (TemporarySoundPlayer audioPlayer in _loopSoundPlayers)
        {
            if (audioPlayer.ClipName == clipName)
            {
                _loopSoundPlayers.Remove(audioPlayer);
                Destroy(audioPlayer.gameObject);
                return;
            }
        }

        Debug.LogWarning(clipName + "을 찾을 수 없습니다.");
    }

    public void PushSoundPlayer(TemporarySoundPlayer soundPlayer)
    {
        soundPlayer.transform.parent = transform;
        _soundPlayerPool.Push(soundPlayer);
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
