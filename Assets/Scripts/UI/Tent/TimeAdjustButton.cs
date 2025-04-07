using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class TimeAdjustButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private TentUI _tentUI;

    [Tooltip("1 -> 30분, 2 -> 1시간)")]
    [SerializeField] private int timeChangeAmount;
    [SerializeField] private float longPressThreshold = 0.5f;
    [Tooltip("단위 : ms")][SerializeField] private int repeatInterval = 200;

    private bool isPressing = false;
    private bool longPressTriggered = false;
    private float pressTime = 0f;

    private void Start()
    {
        if (_tentUI == null)
            _tentUI = FindObjectOfType<TentUI>();
    }

    void Update()
    {
        if (isPressing)
        {
            pressTime += Time.deltaTime;
            if (!longPressTriggered && pressTime >= longPressThreshold)
            {
                longPressTriggered = true;
                RepeatAdjustTime();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressing = true;
        pressTime = 0f;
        longPressTriggered = false;

        _tentUI.AdjustTime(timeChangeAmount);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressing = false;
        longPressTriggered = false;
    }

    private async void RepeatAdjustTime()
    {
        while (isPressing)
        {
            _tentUI.AdjustTime(timeChangeAmount);
            await Task.Delay(repeatInterval);
        }
    }
}
