using TMPro;
using UnityEngine;

public class BattleResultUI : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject resultPanel;         // 결과창 전체 패널
    public TextMeshProUGUI resultTitleText; // "STAGE CLEAR!" 또는 "GAME OVER"
    public TextMeshProUGUI rewardText;

    public void ShowClear(float overkillDamage, int obtainedRelics, int extraGold)
    {
        OpenResultUI();
        resultTitleText.text = "<color=yellow>STAGE CLEAR!</color>";

        int pinataGold = Mathf.FloorToInt(overkillDamage / 100f);
        int totalGold = pinataGold + extraGold;

        string rewardStr = $"피냐타 누적 데미지: {overkillDamage} -> <color=#FFD700>{pinataGold} G</color>\n";

        if (obtainedRelics > 0)
        {
            rewardStr += $"<color=cyan>유물 획득: {obtainedRelics}개!</color>\n";
        }

        if (extraGold > 0)
        {
            rewardStr += $"<color=orange>유물 상한 초과 보상: +{extraGold} G</color>\n";
        }

        rewardStr += $"<b>총 획득 골드: <color=#FFD700>{totalGold} G</color></b>\n";

        rewardText.text = rewardStr;
    }

    public void ShowGameOver()
    {
        OpenResultUI();
        resultTitleText.text = "<color=red>GAME OVER</color>";
        rewardText.text = "보스 토벌에 실패했습니다...\n";
    }

    // 결과창 공통 오픈 로직
    private void OpenResultUI()
    {
        resultPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void OnClickConfirm()
    {
        Time.timeScale = 1f;

        // TODO: 마을(상점) 씬으로 이동하거나 다음 스테이지 로딩
        Debug.Log("다음 화면으로 이동!");
    }
}
