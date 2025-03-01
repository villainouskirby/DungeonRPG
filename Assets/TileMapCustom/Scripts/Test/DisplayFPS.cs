using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayFPS : MonoBehaviour
{
    private TMP_Text fpsText;

    void Start()
    {
        fpsText = GetComponent<TMP_Text>();
    }

    void Update()
    {
        // FPS 계산
        fpsText.text = Mathf.Floor(1 / Time.deltaTime).ToString();
    }
}
