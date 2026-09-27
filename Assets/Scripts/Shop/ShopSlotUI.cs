using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ShopSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 컴포넌트")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI descText;
    public Button buyButton;

    // 공통 매니저 및 플레이어 정보
    private PvpShopManager shopManager;
    private PlayerManager buyer;
    private SlotManager buyerSlotManager; // 유물 장착 시 필요한 보드판 참조

    // 현재 슬롯이 품고 있는 데이터 (둘 중 하나만 채워짐)
    private bool isRelicSlot = false;
    private ItemData currentActiveData;
    private Relic currentRelicData;
    private int currentRelicCost;

    // 액티브(소비) 아이템 셋업
    public void SetupActiveSlot(ItemData data, PvpShopManager manager, PlayerManager player)
    {
        isRelicSlot = false;
        currentActiveData = data;
        currentRelicData = null; // 유물 데이터 비우기

        shopManager = manager;
        buyer = player;

        if (iconImage != null)
        {
            iconImage.sprite = data.itemIcon;
            iconImage.color = Color.white;
        }
        nameText.text = data.itemName;
        costText.text = $"{data.cost}G";
        // descText.text = data.description;

        buyButton.interactable = true;
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnClickBuy);
    }

    // 유물(Relic) 셋업
    public void SetupRelicSlot(Relic relic, PvpShopManager manager, PlayerManager player, SlotManager slotManager)
    {
        isRelicSlot = true;
        currentRelicData = relic;
        currentActiveData = null; // 액티브 데이터 비우기

        shopManager = manager;
        buyer = player;
        buyerSlotManager = slotManager; // 유물은 획득 즉시 장착되므로 타겟 슬롯매니저가 필요함

        if (iconImage != null) iconImage.sprite = relic.icon; // Relic 클래스에 icon 변수가 있어야 함
        nameText.text = relic.relicName;
        costText.text = $"{relic.cost}G";
        // descText.text = relic.description;

        buyButton.interactable = true;
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnClickBuy);
    }

    private void OnClickBuy()
    {
        if (isRelicSlot)
        {
            // 유물 구매 로직 호출
            shopManager.BuyRelic(currentRelicData, buyer, this, buyerSlotManager);
        }
        else
        {
            // 액티브 아이템 구매 로직 호출
            shopManager.BuyActiveItem(currentActiveData, buyer, this);
        }
    }

    public void MarkAsSoldOut()
    {
        buyButton.interactable = false;
        nameText.text = "SOLD OUT";
        if (iconImage != null) iconImage.color = Color.gray;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!buyButton.interactable) return;

        if (isRelicSlot && currentRelicData != null)
        {
            TooltipManager.Instance.ShowTooltip(currentRelicData.relicName, currentRelicData.description);
        }
        else if (!isRelicSlot && currentActiveData != null)
        {
            TooltipManager.Instance.ShowTooltip(currentActiveData.itemName, currentActiveData.description);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Instance.HideTooltip();
    }
}