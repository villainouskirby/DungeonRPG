using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class TextPrinter : MonoBehaviour
{
    [Tooltip("단위 : ms")]
    [SerializeField] private int _delayTime = 100;

    private string _text;
    private TextMeshProUGUI _targetTMP;
    private bool _isPrintingText;

    /// <summary> 지정한 텍스트 출력하기 </summary>
    public void PrintText(TextMeshProUGUI targetTMP, string text)
    {
        _isPrintingText = true;
        _text = text;
        _targetTMP = targetTMP;
        _targetTMP.text = "";

        Print();
    }

    /// <summary>
    /// 텍스트 출력중인지 확인
    /// <para/> 출력중이면 정지 + 한번에 출력
    /// </summary>
    public bool CheckIsPrinting()
    {
        if (_isPrintingText)
        {
            _targetTMP.text = _text;
            _isPrintingText = false;
            return true;
        }

        return false;
    }

    private async void Print()
    {
        _isPrintingText = true;

        for (int i = 0; i < _text.Length; i++)
        {
            _targetTMP.text += _text[i];
            await UniTask.Delay(_delayTime);
        }

        _isPrintingText = false;
    }
}
