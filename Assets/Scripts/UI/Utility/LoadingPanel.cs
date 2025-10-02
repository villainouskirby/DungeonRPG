using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class LoadingPanel : UIBase
{
    [SerializeField] private RectTransform _loadingDots;
    [SerializeField] private float _radius = 300f;
    [SerializeField] private float _cycleInterval = 3f;

    private bool _isStop = false;

    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);

        for (int i = 0; i < _loadingDots.childCount; i++)
        {
            float theta = (float)i / _loadingDots.childCount * 2 * Mathf.PI;
            _loadingDots.GetChild(i).GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Sin(theta), Mathf.Cos(theta)) * _radius;
        }
    }

    [ContextMenu("Start Loading")]
    public void StartLoading()
    {
        _isStop = false;
        gameObject.SetActive(true);

        Loading().Forget();
    }

    private async UniTaskVoid Loading()
    {
        float startTime = Time.time;

        while (!_isStop)
        {
            _loadingDots.rotation = Quaternion.Euler(0, 0, -360 * ((Time.time - startTime) % _cycleInterval / _cycleInterval));
            await UniTask.NextFrame();
        }
    }

    public void StopLoading()
    {
        _isStop = true;

        gameObject.SetActive(false);
    }
}
