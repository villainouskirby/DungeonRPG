using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DialogueRunner : UIBase
{
    [SerializeField] private TextMeshProUGUI _speakerText;
    [SerializeField] private TextMeshProUGUI _lineText;
    [SerializeField] private TextPrinter _printer;

    private Dictionary<DialogueEndEvent.KeyName, Action<string>> _eventDict = new();

    private AsyncOperationHandle<DialogueSO> _handle;
    private Queue<DialogueLineStatement> _dialogueLines;
    private DialogueEndEvent[] _endEvent;

    private bool _isDialogueRunning = false;

    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);
        _printer.InitTMP(_lineText);

        _eventDict[DialogueEndEvent.KeyName.Dialogue] = Init;
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
                    action?.Invoke(endEvent.Value);
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
}