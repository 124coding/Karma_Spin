using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBar : MonoBehaviour
{
    [Header("UI 연결")]
    public Image currentBar;
    public Image nextBar;
    public TextMeshProUGUI phaseText;

    [Header("페이즈별 색상 설정 (배열)")]
    public Color[] phaseColors;

    // BossManager에서 데미지를 입을 때마다 해당 함수 호출
    public void UpdateHealthUI(float currentHP, float maxHP, int currentPhase)
    {
        // 현재 체력바 깍기
        currentBar.fillAmount = currentHP / maxHP;
        
        // 남은 줄 수 텍스트 갱신
        phaseText.text = $"x{currentPhase}";

        // 색상 업데이트
        if(currentPhase > 0)
        {
            currentBar.color = GetColor(currentPhase);

            nextBar.color = GetColor(currentPhase - 1);
        }
        else
        {
            // 피냐타 모드(0줄) 진입 시의 처리
            currentBar.color = Color.gray; // 샌드백 색상
            nextBar.color = Color.white; // 배경색
            phaseText.text = "Dead";
        }
    }

    private Color GetColor(int phase)
    {
        if (phaseColors.Length == 0) return Color.white;

        if (phase <= 0) return new Color(0, 0, 0, 0);

        int colorIndex = (phase - 1) % phaseColors.Length;
        return phaseColors[colorIndex];
    }
}
