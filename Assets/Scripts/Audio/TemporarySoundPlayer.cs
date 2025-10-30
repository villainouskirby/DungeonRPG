using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class TemporarySoundPlayer : MonoBehaviour
{
    private Action OnClipEnd;
    private AudioSource _audioSource;
    public string ClipName => _audioSource.clip.name;

    private bool _isStopped = true;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    public async UniTaskVoid Play(AudioMixerGroup audioMixer, float delay, bool isLoop)
    {
        if (delay > 0)
        {
            await UniTask.WaitForSeconds(delay);
        }

        if (_isStopped)
        {
            SoundManager.Instance.PushSoundPlayer(this);
            return;
        }

        gameObject.SetActive(true);

        _audioSource.outputAudioMixerGroup = audioMixer;
        _audioSource.loop = isLoop;
        _audioSource.Play();

        if (!isLoop)
        {
            ReleaseWhenFinish(_audioSource.clip.length).Forget();
        }
    }

    public void Stop()
    {
        _audioSource.Stop();
        OnClipEnd?.Invoke();
        Reset();

        gameObject.SetActive(false);
        SoundManager.Instance.PushSoundPlayer(this);
    }

    public void StopLoop()
    {
        _audioSource.loop = false;
        ReleaseWhenFinish(_audioSource.clip.length).Forget();
    }

    public void Reset()
    {
        _isStopped = true;
        _audioSource.clip = null;
        _audioSource.loop = false;
        _audioSource.spatialBlend = 0;
        _audioSource.minDistance = 1;
        _audioSource.maxDistance = 500;
        _audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        _audioSource.spread = 0;
        _audioSource.panStereo = 0;
        OnClipEnd = null;
    }

    public void InitSoundClip(AudioClip clip, Action clipEndEvent)
    {
        _audioSource.clip = clip;
        OnClipEnd += clipEndEvent;
        _isStopped = false;
    }

    public void Init3DProperty(float minDistance, float maxDistance, AudioRolloffMode rolloffMode)
    {
        _audioSource.spatialBlend = 1.0f;
        _audioSource.minDistance = minDistance;
        _audioSource.maxDistance = maxDistance;
        _audioSource.rolloffMode = rolloffMode;
        _audioSource.dopplerLevel = 0f;
        _audioSource.spread = 0f;
        _audioSource.panStereo = 0f;
    }
    public void AttachOrSnap(Transform target, bool attachToTarget)
    {
        if (attachToTarget)
        {
            // 부모로 붙이고, 로컬 좌표/회전을 0으로 맞춘다
            transform.SetParent(target, worldPositionStays: false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        else
        {
            // 부모 해제하고, 월드 위치/회전을 즉시 스냅
            transform.SetParent(null);
            transform.position = target.position;
            transform.rotation = target.rotation;
        }
    }
    private async UniTaskVoid ReleaseWhenFinish(float clipLength)
    {
        await UniTask.WaitForSeconds(clipLength);

        while (_audioSource.isPlaying)
        {
            await UniTask.NextFrame();
        }

        Stop();
    }
}
