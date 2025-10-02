using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using DBUtility;
using Events;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class DialogueRunner : UIBase
{
    [SerializeField] private GameObject _buttons;
    [SerializeField] private Button _openUIButton;
    [SerializeField] private Button _talkButton;

    [SerializeField] private TextMeshProUGUI _speakerText;
    [SerializeField] private TextMeshProUGUI _lineText;
    [SerializeField] private TextPrinter _printer;

    private Dictionary<DialogueEndEvent.KeyName, Action<DialogueEndEvent>> _eventDict = new();

    private AsyncOperationHandle<DialogueSO> _handle;
    private Queue<DialogueLineStatement> _dialogueLines;
    private DialogueEndEvent[] _endEvent;

    private Action _openUIAction;
    private string _npcName = "";

    private bool _isDialogueRunning = false;

    protected override void OnDisable()
    {
        _buttons.SetActive(false);
    }

    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);
        _printer.InitTMP(_lineText);

        _eventDict[DialogueEndEvent.KeyName.Dialogue] = ContinueDialogue;
        _eventDict[DialogueEndEvent.KeyName.Get] = AddInventory;
        _eventDict[DialogueEndEvent.KeyName.Lose] = RemoveInventory;
        _eventDict[DialogueEndEvent.KeyName.AcceptQuest] = AddNewQuest;
        _eventDict[DialogueEndEvent.KeyName.UnlockQuest] = UnlockQuest;
        _eventDict[DialogueEndEvent.KeyName.CloseDialogue] = (DialogueEndEvent endEvent) => gameObject.SetActive(false);

        _openUIButton.onClick.AddListener(OpenUI);
        _talkButton.onClick.AddListener(StartTalk);
    }

    public void Init(Action openAction, string npcName)
    {
        _openUIAction = openAction;
        _npcName = npcName;
    }

    private void OpenUI()
    {
        _openUIAction?.Invoke();
    }

    public async UniTaskVoid StartPrint(string dialogueName)
    {
        if (_isDialogueRunning) return;

        if (_handle.IsValid())
        {
            Addressables.Release(_handle);
            _handle = default;
        }

        _isDialogueRunning = true;

        _handle = Addressables.LoadAssetAsync<DialogueSO>("Dialogue/" + dialogueName);
        var dialogue = await _handle.ToUniTask();

        _dialogueLines = new Queue<DialogueLineStatement>(dialogue.Lines);
        _endEvent = dialogue.EndEvent;

        gameObject.SetActive(true);

        TryPrint();
    }

    public void StartTalk()
    {
        /*
        if (string.IsNullOrEmpty(_questName))
        {
            StartPrint(_npcName); // 기본 대사 출력
        }
        else
        {
            StartPrint(_questName); // 퀘스트 대사 출력
        }*/
    }

    private void Update()
    {
        if (_isDialogueRunning && Input.GetMouseButtonUp(0))
        {
            TryPrint();
        }
    }

    private void TryPrint()
    {
        if (!_printer.CheckIsPrinting())
        {
            if (_dialogueLines.TryDequeue(out var statement))
            {
                _speakerText.text = statement.Speaker;
                _printer.Print(statement.Text).Forget();
            }
            else
            {
                EndDialogue();
            }
        }
    }

    private void EndDialogue()
    {
        _isDialogueRunning = false;

        if (_endEvent.Length > 0)
        {
            foreach (var endEvent in _endEvent)
            {
                if (_eventDict.TryGetValue(endEvent.Key, out var action))
                {
                    action?.Invoke(endEvent);
                }
            }
        }

        if (!_isDialogueRunning)
        {
            Addressables.Release(_handle);
            _handle = default;

            if (gameObject.activeSelf)
            {
                _buttons.SetActive(true);
            }
        }
    }

    private void ContinueDialogue(DialogueEndEvent endEvent)
    {
        StartPrint(endEvent.Value).Forget();
    }

    private void AddInventory(DialogueEndEvent endEvent)
    {
        UIPopUpHandler.Instance.GetScript<Inventory>().AddItem(ItemDataConstructor.GetItemData(endEvent.Value), endEvent.Amount);
    }

    private void RemoveInventory(DialogueEndEvent endEvent)
    {
        UIPopUpHandler.Instance.GetScript<Inventory>().RemoveItem(ItemDataConstructor.GetItemData(endEvent.Value), endEvent.Amount);
    }

    private void AddNewQuest(DialogueEndEvent endEvent)
    {
        var info = QuestConstructor.GetQuestInfo(endEvent.Value);

        if (info == null) return;

        UIPopUpHandler.Instance.GetScript<Quest>().AddQuest(info);
    }

    private void UnlockQuest(DialogueEndEvent endEvent)
    {
        using (var args = QuestUnlockedEventArgs.Get())
        {
            var info = Array.Find(Quest_Info.Quest, quest => quest.id == endEvent.Value);

            if (info == null) return;

            args.Init(info.name, info.id);
            EventManager.Instance.QuestUnlockedEvent.Invoke(args);
            args.Release();
        }
    }
}