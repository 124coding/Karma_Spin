using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public Button useButton;

    private ItemData currentItem;
    private PlayerManager owner;
    private BaseBattleManager battleManager;
    private PlayerInventoryUI parentUI;

    public void SetupSlot(ItemData item, PlayerManager player, BaseBattleManager manager, PlayerInventoryUI ui)
    {
        currentItem = item;
        owner = player;
        battleManager = manager;
        parentUI = ui;

        if (iconImage != null && item.itemIcon != null)
        {
            iconImage.sprite = item.itemIcon;
            iconImage.color = Color.white; // 아이콘 보이게
        }

        if (nameText != null) nameText.text = item.itemName;

        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(OnClickUse);
        useButton.interactable = !owner.hasUsedItemThisTurn;
    }

    private void OnClickUse()
    {
        if (!PvpBattleManager.Instance.IsMyTurn) return;

        int slotIndex = parentUI.activeSlots.IndexOf(this);
        if (slotIndex != -1 && NetworkBattleController.Instance != null)
        {
            NetworkBattleController.Instance.RequestItemUse(PvpBattleManager.Instance.myPlayerIndex, slotIndex);
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.color = new Color(0, 0, 0, 0);
        }
        if (nameText != null) nameText.text = "Empty";
        useButton.interactable = false;
        useButton.onClick.RemoveAllListeners();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(currentItem != null && TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip(currentItem.itemName, currentItem.description);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
}
