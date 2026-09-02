using UnityEngine;
using TMPro;
public class BattleLogUI : MonoBehaviour
{
    public TextMeshProUGUI logText;

    // 매 스핀(턴)이 시작될 때 로그 창을 싹 비워줍니다.
    public void ClearLog()
    {
        logText.text = "";
    }

    // 새로운 로그를 한 줄씩 추가합니다.
    public void AddLog(string message)
    {
        // 기존 텍스트 뒤에 줄바꿈(\n)과 함께 새 메시지를 붙입니다.
        logText.text += message + "\n";
    }
}