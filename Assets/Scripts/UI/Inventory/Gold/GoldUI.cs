using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _goldText;

    public void ChangeValue(int amount)
    {
        _goldText.text = amount.ToString();
    }
}
