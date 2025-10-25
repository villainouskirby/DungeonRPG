using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class TemporarySoundPlayer : MonoBehaviour
{
    private AudioSource _audioSource;
    public string ClipName => _audioSource.clip.name;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public async UniTaskVoid Play(AudioMixerGroup audioMixer, float delay, bool isLoop)
    {
        if (delay > 0)
        {
            await UniTask.WaitForSeconds(delay);
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

    public void InitSound2D(AudioClip clip)
    {
        _audioSource.clip = clip;
    }

    public void InitSound3D(AudioClip clip, float minDistance, float maxDistance)
    {
        _audioSource.clip = clip;
        _audioSource.spatialBlend = 1.0f;
        _audioSource.rolloffMode = AudioRolloffMode.Linear;
        _audioSource.minDistance = minDistance;
        _audioSource.maxDistance = maxDistance;
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

        gameObject.SetActive(false);
        SoundManager.Instance.PushSoundPlayer(this);
    }
}
