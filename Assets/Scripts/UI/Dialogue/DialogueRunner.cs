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

    private AsyncOperationHandle<DialogueSO> _handle;
    private Queue<DialogueLineStatement> _dialogueLines;
    private string _endEventKey;

    private bool _isDialogueRunning = false;

    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);
        _printer.InitTMP(_lineText);
    }

    public void Init(string dialogueName)
    {
        _handle = Addressables.LoadAssetAsync<DialogueSO>("Dialogue/" + dialogueName);
        var dialogue = _handle.WaitForCompletion();

        _dialogueLines = new Queue<DialogueLineStatement>(dialogue.Lines);
        _endEventKey = dialogue.EndEventKey;

        _isDialogueRunning = true;

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

        if (!string.IsNullOrEmpty(_endEventKey))
        {
            // end 이벤트 실행
        }

        Addressables.Release(_handle);

        _handle = default;
    }
}