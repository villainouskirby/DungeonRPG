using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using DBUtility;
using Events;

public class DialogueRunner : UIBase
{
    [SerializeField] private TextMeshProUGUI _speakerText;
    [SerializeField] private TextMeshProUGUI _lineText;
    [SerializeField] private TextPrinter _printer;

    private Dictionary<DialogueEndEvent.KeyName, Action<DialogueEndEvent>> _eventDict = new();

    private AsyncOperationHandle<DialogueSO> _handle;
    private Queue<DialogueLineStatement> _dialogueLines;
    private DialogueEndEvent[] _endEvent;

    private bool _isDialogueRunning = false;

    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);
        _printer.InitTMP(_lineText);

        _eventDict[DialogueEndEvent.KeyName.Dialogue] = ContinueDialogue;
        _eventDict[DialogueEndEvent.KeyName.Get] = AddInventory;
        _eventDict[DialogueEndEvent.KeyName.Lose] = RemoveInventory;
        _eventDict[DialogueEndEvent.KeyName.AcceptQuest] = AddNewQuest;
        _eventDict[DialogueEndEvent.KeyName.UnlockQuest] = UnlockQuest;
    }

    public void Init(string dialogueName)
    {
        _handle = Addressables.LoadAssetAsync<DialogueSO>("Dialogue/" + dialogueName);
        var dialogue = _handle.WaitForCompletion();

        _dialogueLines = new Queue<DialogueLineStatement>(dialogue.Lines);
        _endEvent = dialogue.EndEvent;

        _isDialogueRunning = true;

        gameObject.SetActive(true);

        TryPrint();
    }

    private void Update()
    {
        if (_isDialogueRunning && Input.GetMouseButton(0))
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

        Addressables.Release(_handle);

        _handle = default;

        if (!_isDialogueRunning)
        {
            gameObject.SetActive(false);
        }
    }

    private void ContinueDialogue(DialogueEndEvent endEvent)
    {
        Init(endEvent.Value);
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
        UIPopUpHandler.Instance.GetScript<Quest>().AddQuest(QuestConstructor.GetQuestInfo(endEvent.Value));
    }

    private void UnlockQuest(DialogueEndEvent endEvent)
    {
        using (var args = QuestUnlockedEventArgs.Get())
        {
            var info = Array.Find(Quest_Info.Quest, quest => quest.id == endEvent.Value);
            args.Init(info.name, info.id);
            EventManager.Instance.QuestUnlockedEvent.Invoke(args);
            args.Release();
        }
    }
}