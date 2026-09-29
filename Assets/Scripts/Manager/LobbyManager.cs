using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode; // Netcode 추가
using System.Threading.Tasks;

public class LobbyManager : MonoBehaviour
{
    [Header("메인 화면 UI")]
    public Button findGameButton;        // 빠른 매칭 버튼
    public Button createPrivateButton;   // 비공개 방 만들기 버튼
    public Button joinPrivateButton;     // 비공개 방 참여 버튼
    public TMP_InputField joinCodeInput; // 참여 코드 입력창

    [Header("매칭 중 팝업 UI")]
    public GameObject matchingWindowPanel;
    public TextMeshProUGUI matchingStatusText;
    public TextMeshProUGUI joinCodeDisplayText;
    public Button cancelMatchingButton;

    private void Start()
    {
        if (matchingWindowPanel != null) matchingWindowPanel.SetActive(false);
        if (joinCodeDisplayText != null) joinCodeDisplayText.text = "";

        if (MatchmakingManager.Instance != null)
        {
            MatchmakingManager.Instance.OnMatchStateChanged += UpdateMatchingText;
            MatchmakingManager.Instance.OnMatchSuccess += HandleMatchSuccess;
            MatchmakingManager.Instance.OnMatchFailed += HandleMatchFailed;
        }
    }

    public void OnClickFindGame()
    {
        SetMatchingUI(true, "매칭 중...");
        MatchmakingManager.Instance.StartAutoMatchmaking();
    }

    public async void OnClickCreatePrivate()
    {
        SetMatchingUI(true, "비공개 방 생성 중...");

        // MatchmakingManager의 Task가 끝날 때까지 기다림
        await MatchmakingManager.Instance.CreatePrivateRoom();

        // 방 생성이 완료되면 발급된 코드를 화면에 표시
        string code = MatchmakingManager.Instance.JoinCode;
        if (!string.IsNullOrEmpty(code))
        {
            matchingStatusText.text = "Join Code: ";
            joinCodeDisplayText.text = $"[ {code} ]";
        }
        else
        {
            matchingStatusText.text = "방 생성에 실패했습니다.";
        }
    }

    public void OnClickJoinPrivate()
    {
        string code = joinCodeInput.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("코드를 먼저 입력하세요!");
            return;
        }

        SetMatchingUI(true, $"코드 [{code}]로 접속 중...");
        MatchmakingManager.Instance.JoinPrivateRoom(code);
    }

    public void OnClickCancelMatching()
    {
        // Netcode 통신 강제 종료 (방장이면 서버를 내리고, 참가자면 방에서 나감)
        if (NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        SetMatchingUI(false, "");
        Debug.Log("매칭을 취소했습니다.");
    }

    // 버튼 활성화/비활성화 및 팝업 텍스트를 한 번에 관리하는 헬퍼 함수
    private void SetMatchingUI(bool isMatching, string msg)
    {
        findGameButton.interactable = !isMatching;
        if (createPrivateButton != null) createPrivateButton.interactable = !isMatching;
        if (joinPrivateButton != null) joinPrivateButton.interactable = !isMatching;

        matchingWindowPanel.SetActive(isMatching);
        if (matchingStatusText != null) matchingStatusText.text = msg;
        if (joinCodeDisplayText != null) joinCodeDisplayText.text = ""; // 코드 텍스트 초기화
    }

    private void UpdateMatchingText(string msg)
    {
        if (matchingStatusText != null) matchingStatusText.text = msg;
    }

    private void HandleMatchSuccess()
    {
        cancelMatchingButton.interactable = false;
    }

    private void HandleMatchFailed()
    {
        SetMatchingUI(false, "");
        Debug.LogWarning("접속에 실패하여 매칭이 취소되었습니다.");
    }
}

// NetworkTest 사용 시
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

//public class LobbyManager : MonoBehaviour
//{
//    [Header("메인 화면 UI ")]
//    public Button findGameButton;

//    [Header("매칭 중 팝업 UI")]
//    public GameObject matchingWindowPanel; // 매칭 중 창 (Panel)
//    public TextMeshProUGUI matchingStatusText; // 상태 표시 텍스트
//    public Button cancelMatchingButton; // 매칭 취소 버튼

//    private void Start()
//    {
//        if (matchingWindowPanel != null) matchingWindowPanel.SetActive(false);

//        if(NetworkTest.Instance != null)
//        {
//            NetworkTest.Instance.OnMatchingStateChanged += UpdateMatchingText;
//            NetworkTest.Instance.OnConnectionFailed += HandleConnectionFailed;
//        }
//    }

//    public void OnClickFindGame()
//    {
//        findGameButton.interactable = false;
//        matchingWindowPanel.SetActive(true);
//        matchingStatusText.text = "서버 접속 및 상대 찾는 중...";

//        // NetworkTest 싱글톤이 없으면 씬에 있는 걸 씀
//        if (NetworkTest.Instance != null)
//        {
//            NetworkTest.Instance.ConnectToServer();
//        }
//    }

//    public void OnClickCancelMatching()
//    {
//        if(NetworkTest.Instance != null)
//        {
//            NetworkTest.Instance.DisconnectFromServer();
//        }

//        matchingWindowPanel.SetActive(false);
//        findGameButton.interactable = true;
//        Debug.Log("매칭을 취소했습니다");
//    }

//    private void UpdateMatchingText(string msg)
//    {
//        if (matchingStatusText != null)
//        {
//            matchingStatusText.text = msg;
//        }
//    }

//    // 서버가 꺼져있거나 접속에 실패했을 때 복구
//    private void HandleConnectionFailed()
//    {
//        matchingWindowPanel.SetActive(false);
//        findGameButton.interactable = true;
//        Debug.LogWarning("서버 접속에 실패하여 매칭이 취소되었습니다.");
//    }
//}