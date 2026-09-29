using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RelicSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    private Relic currentRelic;

    public void SetupSlot(Relic relic)
    {
        currentRelic = relic;

        if (iconImage != null && relic.icon != null)
        {
            iconImage.sprite = relic.icon;
            iconImage.color = Color.white;
        }
    }

    public void ClearSlot()
    {
        currentRelic = null;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.color = new Color(0, 0, 0, 0);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentRelic != null && TooltipManager.Instance != null)
        {
            // Relic 스크립터블 오브젝트에 정의된 이름과 설명 사용
            TooltipManager.Instance.ShowTooltip(currentRelic.relicName, currentRelic.description);
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