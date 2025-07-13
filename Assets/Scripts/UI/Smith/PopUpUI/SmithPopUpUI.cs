using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SmithPopUpUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _stats;
    [SerializeField] private Transform _ingredients;
    [SerializeField] private Button _craftButton;

    [Header("Smith UI")]
    [SerializeField] private SmithUI _smithUI;

    [Header("Confirm UI")]
    [SerializeField] private SmithConfirmPopUpUI _confirmPopUpUI;

    private GameObject[] _ingredientObjects;
    private Image[] _ingredientImages;
    private TextMeshProUGUI[] _ingredientTexts;

    private SmithData _selectedData;

    public void InitPopUpUI()
    {
        if (_ingredientObjects != null) return;

        _ingredientObjects = new GameObject[4];
        _ingredientImages = new Image[4];
        _ingredientTexts = new TextMeshProUGUI[4];

        for (int i = 0; i < 4; i++)
        {
            Transform ingredient = _ingredients.GetChild(i);

            _ingredientObjects[i] = ingredient.gameObject;

            GameObject imgObject = ingredient.gameObject;
            Image image = imgObject.GetOrAddComponent<Image>();
            _ingredientImages[i] = image;

            GameObject txtObject = ingredient.GetChild(0).gameObject;
            TextMeshProUGUI text = txtObject.GetOrAddComponent<TextMeshProUGUI>();
            _ingredientTexts[i] = text;
        }
    }

    public void SetInfo(int index)
    {
        _craftButton.enabled = false; // 혹시 모를 버그 방지로 일단 버튼 비활성화

        _selectedData = _smithUI.GetSmithData(index);
        bool canCraft = true;

        for (int i = 0; i < 4; i++)
        {
            // 재료의 수량이 0 => 필요없으니 비활성화
            if (_selectedData.IngredientAmounts[i] == 0)
            {
                _ingredientObjects[i].SetActive(false);
            }
            else
            {
                // TODO => DB랑 연결해서 아이템데이터와 현재 가진 수량 같은거 가져와서 띄우도록 하기
                // ID임시수정 - 일단은 주석 처리..
                int ingredientAmount = 1;
                //int ingredientAmount = _smithUI.GetIngredientAmount(_selectedData.IngredientItemIDs[i]);
                _ingredientTexts[i].text = "\n" + ingredientAmount.ToString();
                _ingredientTexts[i].text += "/" + _selectedData.IngredientAmounts[i].ToString();
                _ingredientObjects[i].SetActive(true);

                if (canCraft && ingredientAmount < _selectedData.IngredientAmounts[i])
                {
                    canCraft = false;
                }
            }
        }

        if (canCraft)
        {
            _craftButton.enabled = true;
        }
        else
        {
            _craftButton.enabled = false;
        }

        //_outputItem = data.ResultItemID;
        gameObject.SetActive(true);
    }

    public void CraftItem()
    {
        if (!_smithUI.CheckCanCraft()) return; // 인벤 여유공간 확보

        ItemData data; // DB에서 ResultItemID에 해당하는 데이터 값 가져오기
        //_confirmPopUpUI.OpenConfirmPopUpUI(data.IconSprite, data.Name);
    }

    /// <summary> 제작된 아이템 인벤토리에 추가 </summary>
    public void AddCraftedItemToInventory()
    {
        _smithUI.AddCraftedItemToInventory(_selectedData);
    }

    /// <summary> 방금 인벤에 들어간 장비 사용(장착) </summary>
    public void EquipItem()
    {
        _smithUI.EquipItem();
    }
}
