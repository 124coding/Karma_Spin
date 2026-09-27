using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [Header("메인 화면 UI ")]
    public Button findGameButton;

    [Header("매칭 중 팝업 UI")]
    public GameObject matchingWindowPanel; // 매칭 중 창 (Panel)
    public TextMeshProUGUI matchingStatusText; // 상태 표시 텍스트
    public Button cancelMatchingButton; // 매칭 취소 버튼

    private void Start()
    {
        if (matchingWindowPanel != null) matchingWindowPanel.SetActive(false);

        if(NetworkTest.Instance != null)
        {
            NetworkTest.Instance.OnMatchingStateChanged += UpdateMatchingText;
            NetworkTest.Instance.OnConnectionFailed += HandleConnectionFailed;
        }
    }

    public void OnClickFindGame()
    {
        findGameButton.interactable = false;
        matchingWindowPanel.SetActive(true);
        matchingStatusText.text = "서버 접속 및 상대 찾는 중...";

        // NetworkTest 싱글톤이 없으면 씬에 있는 걸 씀
        if (NetworkTest.Instance != null)
        {
            NetworkTest.Instance.ConnectToServer();
        }
    }

    public void OnClickCancelMatching()
    {
        if(NetworkTest.Instance != null)
        {
            NetworkTest.Instance.DisconnectFromServer();
        }

        matchingWindowPanel.SetActive(false);
        findGameButton.interactable = true;
        Debug.Log("매칭을 취소했습니다");
    }

    private void UpdateMatchingText(string msg)
    {
        if (matchingStatusText != null)
        {
            matchingStatusText.text = msg;
        }
    }

    // 서버가 꺼져있거나 접속에 실패했을 때 복구
    private void HandleConnectionFailed()
    {
        matchingWindowPanel.SetActive(false);
        findGameButton.interactable = true;
        Debug.LogWarning("서버 접속에 실패하여 매칭이 취소되었습니다.");
    }
}
