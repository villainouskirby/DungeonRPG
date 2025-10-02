using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;


public class ItemGetPopUpUI : MonoBehaviour
{
    private struct PopUpItemInfo
    {
        public Sprite Sprite;
        public string Name;
        public int Amount;

        public PopUpItemInfo(Sprite sprite, string name, int amount)
        {
            this.Sprite = sprite;
            this.Name = name;
            this.Amount = amount;
        }
    };

    [SerializeField] private ItemGetPopUpSlotUI[] _popUpSlotArr;
    [SerializeField] private RectTransform _content;

    private Stack<ItemGetPopUpSlotUI> _popUpSlotStack = new();
    private Queue<ItemGetPopUpSlotUI> _popUpSlotQueue = new();
    private Queue<PopUpItemInfo> _itemQueue = new();

    private bool _isProcessingQueue = false;
    private bool _isCleaned = false;

    private CancellationTokenSource _cts;

    private void Awake()
    {
        foreach (var slot in _popUpSlotArr)
        {
            if (slot == null) continue;

            slot.InitSlotUI();
            _popUpSlotStack.Push(slot);
        }

        _cts = new CancellationTokenSource();
    }

    /// <summary> 아이템 획득시 팝업될 아이템 큐에 추가 </summary>
    public void AddItemPopUpQueue(ItemData itemData, int amount = 1)
    {
        _itemQueue.Enqueue(new PopUpItemInfo(itemData.IconSprite, itemData.Name, amount));

        if (!_isProcessingQueue)
        {
            ProcessItemQueue().Forget();
        }
    }

    private void PushBackToStack()
    {
        _popUpSlotStack.Push(_popUpSlotQueue.Dequeue());
    }

    public async UniTask ProcessItemQueue()
    {
        _isProcessingQueue = true;

        while (_itemQueue.Count > 0)
        {
            // 슬롯이 다 찬 상태에서 팝업 요청이 있을 때
            if (_popUpSlotStack.Count == 0)
            {
                await _popUpSlotQueue.Peek().ClosePopUp(_cts.Token);
            }

            // 이미 슬롯이 있으면 위로 올리기
            if (_popUpSlotStack.Count < 3)
            {
                float startTime = Time.time;
                float dTime;

                float startPosY = _content.anchoredPosition.y;
                float destPosY = startPosY + 150;

                while ((dTime = (Time.time - startTime) / 0.1f) < 1)
                {
                    _content.anchoredPosition = Vector2.up * Mathf.Lerp(startPosY, destPosY, dTime);

                    await UniTask.NextFrame(cancellationToken: _cts.Token);
                }

                _content.anchoredPosition = Vector2.up * destPosY;
            }

            var slot = _popUpSlotStack.Pop();
            var info = _itemQueue.Dequeue();
            slot.SetItemInfo(info.Sprite, info.Name, info.Amount);
            _popUpSlotQueue.Enqueue(slot);

            await slot.PopUpSlot(_content.anchoredPosition.y, 0.2f, _cts.Token);
            slot.OnPopUpClose += PushBackToStack;
        }

        _isProcessingQueue = false;
    }

    private void CleanAsyncTasks()
    {
        if (_isCleaned) return;
        _isCleaned = true;

        if (_cts != null)
        {
            if (!_cts.IsCancellationRequested)
                _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private void OnDestroy()
    {
        CleanAsyncTasks();
    }

    private void OnApplicationQuit()
    {
        CleanAsyncTasks();
    }
}
