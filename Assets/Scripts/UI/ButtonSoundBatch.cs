#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundBatch
{
    [MenuItem("Tools/Add Click Sound To All Buttons")]
    public static void AddClickSound()
    {
        var buttons = Object.FindObjectsOfType<Button>(true);
        foreach (var b in buttons)
        {
            var audio = b.gameObject.GetComponent<ButtonClickSound>();
            if (audio == null)
            {
                b.gameObject.AddComponent<ButtonClickSound>();
            }
        }

        Debug.Log($"사운드 스크립트 {buttons.Length}개 버튼에 자동 추가 완료!");
    }
}
#endif