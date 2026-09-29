using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("ÅøÆÁ UI ¿ä¼Ò")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;

    private RectTransform rectTransform;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(Instance);

        rectTransform = tooltipPanel.GetComponent<RectTransform>();
        HideTooltip();
    }

    private void Update()
    {
        if (tooltipPanel.activeSelf)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            float screenRatioX = mousePos.x / Screen.width;
            float screenRatioY = mousePos.y / Screen.height;

            float pivotX = screenRatioX > 0.6f ? 1f : 0f;
            float pivotY = screenRatioY > 0.5f ? 1f : 0f;

            rectTransform.pivot = new Vector2(pivotX, pivotY);

            float offsetX = pivotX == 0f ? 15 : -15f;
            float offsetY = pivotY == 0f ? -15 : 15f;

            tooltipPanel.transform.position = mousePos + new Vector2(offsetX, offsetY);
        }
    }

    public void ShowTooltip(string itemName, string itemDesc)
    {
        nameText.text = itemName;
        descText.text = itemDesc;
        tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}
