using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SmithPopUpUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _gatheringTier;
    [SerializeField] private Transform _ingredients;
    [SerializeField] private Button _createButton;

    private GameObject[] _ingredientObjects;
    private Image[] _ingredientImages;
    private TextMeshProUGUI[] _ingredientTexts;

    private void Awake()
    {
        _ingredientImages = new Image[4];
        _ingredientTexts = new TextMeshProUGUI[4];

        for (int i = 0; i < 4; i++)
        {
            Transform ingredient = _ingredients.GetChild(i);

            _ingredientObjects[i] = ingredient.gameObject;

            GameObject imgObject = ingredient.GetChild(0).gameObject;
            Image image = imgObject.GetOrAddComponent<Image>();
            _ingredientImages[i] = image;

            GameObject txtObject = ingredient.GetChild(1).gameObject;
            TextMeshProUGUI text = txtObject.GetOrAddComponent<TextMeshProUGUI>();
            _ingredientTexts[i] = text;
        }
    }

    public void SetInfo(SmithData data)
    {
        for (int i = 0; i < 4; i++)
        {
            // 재료의 수량이 0 => 필요없으니 비활성화
            if (data.IngredientAmounts[i] == 0)
            {
                _ingredientObjects[i].SetActive(false);
            }
            else
            {
                // TODO => DB랑 연결해서 아이템데이터와 현재 가진 수량 같은거 가져와서 띄우도록 하기

                _ingredientObjects[i].SetActive(true);
            }
        }

        gameObject.SetActive(true);
    }
}
