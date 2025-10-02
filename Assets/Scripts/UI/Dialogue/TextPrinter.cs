using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class TextPrinter : MonoBehaviour
{
    [Tooltip("단위 : ms")]
    [SerializeField] private int _delayTime = 100;

    private string _text;
    private TextMeshProUGUI _targetTMP;
    private bool _isPrintingText;

    private CancellationTokenSource _cts;

    private void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _isPrintingText = false;
    }

    public void InitTMP(TextMeshProUGUI targetTMP)
    {
        _targetTMP = targetTMP;
        _targetTMP.text = "";
    }

    /// <summary>
    /// 텍스트 출력중인지 확인
    /// <para/> 출력중이면 정지 + 한번에 출력
    /// </summary>
    public bool CheckIsPrinting()
    {
        if (_isPrintingText)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _targetTMP.text = _text;
            _isPrintingText = false;
            return true;
        }

        return false;
    }

    public async UniTaskVoid Print(string text)
    {
        _cts = new CancellationTokenSource();
        _text = text;

        _targetTMP.text = "";

        _isPrintingText = true;

        try
        {
            for (int i = 0; i < _text.Length; i++)
            {
                _targetTMP.text += _text[i];
                await UniTask.Delay(_delayTime, cancellationToken: _cts.Token);
            }
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            _isPrintingText = false;
            _cts?.Dispose();
            _cts = null;
        }
    }
}
