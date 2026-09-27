using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PvpUIManager : MonoBehaviour
{
    [Header("화면 Dim 처리")]
    public GameObject dimPanel1P;
    public GameObject dimPanel2P;

    [Header("인벤토리 UI")]
    public PlayerInventoryUI inventoryUI;

    public TextMeshProUGUI goldText;

    [Header("PvP 중앙 격돌 UI")]
    public Slider tugOfWarSlider;
    public TextMeshProUGUI tugGaugeText;
    public TextMeshProUGUI pendingDamageText; // 선턴 텍스트
    public TextMeshProUGUI secondDamageText;  // 후턴 텍스트
    public Button centralSpinButton;
    public TextMeshProUGUI centralSpinText;

    [Header("각 쉴드 UI")]
    public TextMeshProUGUI p1ShieldText;
    public TextMeshProUGUI p2ShieldText;

    [Header("라운드 스코어 UI")]
    public TextMeshProUGUI p1WinCountText;
    public TextMeshProUGUI p2WinCountText;

    [Header("결과 UI")]
    public GameObject finalResultPanel;
    public TextMeshProUGUI finalWinnerText;

    [Header("연결 끊김 안내 팝업")]
    public GameObject disconnectPanel;
    public TextMeshProUGUI disconnectDescText;


    public void SetupGauge(float maxValue)
    {
        if (tugOfWarSlider != null)
        {
            tugOfWarSlider.minValue = -maxValue;
            tugOfWarSlider.maxValue = maxValue;
            tugOfWarSlider.value = 0f;
        }

        HideDamageTexts();
    }

    public void UpdateTugGaugeText(float tugGauge)
    {
        if (tugOfWarSlider != null)
        {
            tugOfWarSlider.value = tugGauge;
        }

        if (tugGaugeText == null) return;

        if (tugGauge < 0)
        {
            tugGaugeText.text = $"{Mathf.Abs(tugGauge)}";
            tugGaugeText.color = Color.cyan;
        }
        else if (tugGauge > 0)
        {
            tugGaugeText.text = $"{tugGauge}";
            tugGaugeText.color = new Color(1f, 0.4f, 0.4f);
        }
        else
        {
            tugGaugeText.text = $"{0}";
            tugGaugeText.color = Color.white;
        }
    }

    public void ShowPendingDamage(float damage, bool isP1Turn)
    {
        pendingDamageText.text = damage.ToString();
        pendingDamageText.color = isP1Turn ? Color.cyan : new Color(1f, 0.4f, 0.4f);
        pendingDamageText.gameObject.SetActive(true);
    }

    public void ShowSecondDamage(float damage, bool isP1Turn)
    {
        secondDamageText.text = damage.ToString();
        secondDamageText.color = isP1Turn ? Color.cyan : new Color(1f, 0.4f, 0.4f);
        secondDamageText.gameObject.SetActive(true);
    }

    public void HideDamageTexts()
    {
        if(pendingDamageText != null)
        {
            pendingDamageText.text = "";
            pendingDamageText.gameObject.SetActive(false);
        }

        if(secondDamageText != null)
        {
            secondDamageText.text = "";
            secondDamageText.gameObject.SetActive(false);
        }
    }

    public void UpdateBoardDimState(bool isP1Turn)
    {
        if(dimPanel1P != null) dimPanel1P.SetActive(!isP1Turn);
        if(dimPanel2P != null) dimPanel2P.SetActive(isP1Turn);

        centralSpinButton.image.color = isP1Turn ? Color.cyan : new Color(1f, 0.4f, 0.4f);
        centralSpinText.text = isP1Turn ? "1P SPIN" : "2P SPIN";
    }

    public void SetSpinButtonInteractable(bool interactable)
    {
        centralSpinButton.interactable = interactable;
    }

    public void SetGoldText(int amount)
    {
        goldText.text = amount > 0 ? amount.ToString() + "G" : "0G";
    }

    public void SetP1ShieldText(float amount)
    {
        p1ShieldText.text = amount > 0 ? amount.ToString("F0") : "";
    }

    public void SetP2ShieldText(float amount)
    {
        p2ShieldText.text = amount > 0 ? amount.ToString("F0") : "";
    }

    public void SetP1WinCountText(int winCount)
    {
        p1WinCountText.text = winCount.ToString();
    }

    public void SetP2WinCountText(int winCount)
    {
        p2WinCountText.text = winCount.ToString();
    }

    public void ShowFinalResult(bool isVictory)
    {
        if (finalResultPanel != null)
        {
            finalResultPanel.SetActive(true);

            if (isVictory)
            {
                finalWinnerText.text = "VICTORY!";
                finalWinnerText.color = Color.cyan;
            }
            else
            {
                finalWinnerText.text = "DEFEAT";
                finalWinnerText.color = new Color(1f, 0.4f, 0.4f);
            }
        }
    }

    public void HideFinalResults()
    {
        if (finalResultPanel != null) { finalResultPanel.SetActive(false); }
    }

    public void RefreshAllInventoryUI()
    {
        inventoryUI.RefreshInventoryUI();
    }

    public void ShowOpponentDisconnectedUI()
    {
        SetSpinButtonInteractable(false);

        if(disconnectPanel != null)
        {
            disconnectPanel.SetActive(true);

            if (disconnectDescText != null) disconnectDescText.text = "상대방과의 연결이 끊어졌습니다.\n";
        }
    }

    public void OnClickReturnToLobby()
    {
        if(NetworkTest.Instance != null)
        {
            NetworkTest.Instance.DisconnectFromServer();
        }

        SceneManager.LoadScene("LobbyScene");
    }
}
