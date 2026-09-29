//using System;
//using System.Net.Sockets;
//using System.Threading.Tasks;
//using System.Text;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using System.Collections.Generic;

//// 통신에 사용할 규격(패킷) 설계
//[Serializable]
//public class GamePacket
//{
//    public int packetType;
//    public int player; // 1: 1P, 2: 2P
//    public string data; // 추가 메시지나 아이템 ID 등
//}

//[Serializable]
//public class ShopSyncData
//{
//    public List<string> relicNames = new List<string>();
//    public List<string> activeItemNames = new List<string>();
//}

//public class NetworkTest : MonoBehaviour
//{
//    public static NetworkTest Instance;
//    private TcpClient client;
//    private NetworkStream stream;

//    public int myPlayerIndex = 0;
//    public int savedGameSeed = 0;

//    // 백그라운드 수신부와 유니티 메인 스레드를 연결해주는 큐
//    private readonly Queue<Action> executeOnMainThread = new Queue<Action>();

//    // 로비 UI에 상태를 전달할 이벤트
//    public event Action<string> OnMatchingStateChanged;
//    public event Action OnConnectionFailed;

//    private void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//        }
//        else
//        {
//            Destroy(gameObject); // 로비로 돌아왔을 때 중복 생성 방지
//        }
//    }

//    private void Update()
//    {
//        // 수신부에서 큐에 넣어둔 작업들을 유니티 메인 스레드에서 실행
//        while(executeOnMainThread.Count > 0)
//        {
//            executeOnMainThread.Dequeue().Invoke();
//        }
//    }

//    // 비동기 작업을 위해 async void를 사용 (서버 응답 기다리는 동안 유니티 화면 멈추는 것 방지
//    public async void ConnectToServer()
//    {
//        if (client != null && client.Connected) return;

//        client = new TcpClient();
//        Debug.Log("[클라이언트] 서버 접속 시도");
//        try
//        {
//            // 내 컴퓨터의 7777 포트로 접속 요청
//            await client.ConnectAsync("", 7777);
//            Debug.Log("<color=green>[클라이언트] 서버 접속 성공!</color>");

//            // 데이터 통로 연결
//            stream = client.GetStream();

//            // 서버 응답을 기다리는 수신 함수를 백그라운드에 띄워둠
//            ReceiveMessages();
//        }
//        catch(Exception e)
//        {
//            Debug.LogError($"[클라이언트] 서버 접속 실패: {e.Message}");
//            OnConnectionFailed?.Invoke();
//        }
//    }

//    private async void ReceiveMessages()
//    {
//        byte[] buffer = new byte[1024]; // 서버가 보낼 데이터를 담을 바구니

//        try
//        {
//            while (true)
//            {
//                // 서버가 데이터를 보낼 때까지 대기 (비동기)
//                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
//                if (bytesRead == 0) break;

//                // 바이트를 다시 문자열로 변환(디코딩)하여 출력
//                string rawMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

//                // 줄바꿈 기호를 기준으로 쪼개기
//                string[] jsonMessages = rawMessage.Split('\n');

//                foreach (string jsonMessage in jsonMessages)
//                {
//                    // 빈 문자열은 무시
//                    if (string.IsNullOrEmpty(jsonMessage)) continue;

//                    // 서버에서 온 JSON 문자열을 C# 클래스로 조립 (역직렬화)
//                    GamePacket receivedPacket = JsonUtility.FromJson<GamePacket>(jsonMessage);

//                    // 메인 스레드 큐에 실행할 코드를 캡슐화하여 예약
//                    executeOnMainThread.Enqueue(() =>
//                    {
//                        if (receivedPacket.packetType == 0)
//                        {
//                            if (receivedPacket.data == "Wait")
//                            {
//                                Debug.Log("상대방을 기다리는 중...");
//                                OnMatchingStateChanged?.Invoke("상대방을 기다리는 중...");
//                            }
//                            else
//                            {
//                                // 매칭 성공 및 난수 동기화 처리
//                                myPlayerIndex = receivedPacket.player;
//                                savedGameSeed = int.Parse(receivedPacket.data); // 서버가 보낸 시드 꺼내기

//                                Debug.Log($"매칭 성공! {myPlayerIndex}P로 게임 씬 진입");

//                                SceneManager.sceneLoaded += OnGameSceneLoaded;
//                                SceneManager.LoadScene("BattleScene");
//                            }
//                        }
//                        else if (receivedPacket.packetType == 1)
//                        {
//                            int currentTurnPlayer = receivedPacket.player;
//                            int resetSeed = int.Parse(receivedPacket.data);

//                            if (PvpBattleManager.Instance != null)
//                                PvpBattleManager.Instance.UpdateTurnState(currentTurnPlayer, resetSeed);
//                        }
//                        else if (receivedPacket.packetType == 2)
//                        {
//                            int actionPlayer = receivedPacket.player;

//                            if (PvpBattleManager.Instance != null)
//                            {
//                                PvpBattleManager.Instance.ExecuteNetworkSpin(actionPlayer);
//                            }
//                        }
//                        else if (receivedPacket.packetType == 3)
//                        {
//                            int actionPlayer = receivedPacket.player;

//                            string[] splitData = receivedPacket.data.Split('_');

//                            int slotIndex = int.Parse(splitData[0]);
//                            int reelSeed = int.Parse(splitData[1]);

//                            if (PvpBattleManager.Instance != null)
//                            {
//                                PvpBattleManager.Instance.ExecuteNetworkItemUse(actionPlayer, slotIndex, reelSeed);
//                            }
//                        }
//                        else if (receivedPacket.packetType == 4)
//                        {
//                            int readyPlayer = receivedPacket.player;

//                            // 내가 아닌 상대방이 레디 패킷을 보냈다면 UI 업데이트
//                            if (readyPlayer != myPlayerIndex && PvpBattleManager.Instance != null)
//                            {
//                                PvpBattleManager.Instance.OnOpponentReady(receivedPacket.data);
//                            }
//                        }
//                        else if (receivedPacket.packetType == 5)
//                        {
//                            int roundSeed = int.Parse(receivedPacket.data);
//                            if (PvpBattleManager.Instance != null)
//                            {
//                                PvpBattleManager.Instance.StartNextRoundFromNetwork(roundSeed);
//                            }
//                        }
//                        else if (receivedPacket.packetType == 99)
//                        {
//                            Debug.LogWarning("[네트워크] 서버로부터 상대방 퇴장 알림 수신");

//                            if (PvpBattleManager.Instance != null)
//                            {
//                                PvpBattleManager.Instance.HandleOpponentDisconnected();
//                            }
//                        }
//                    });
//                }
//            }
//        }
//        catch(Exception e)
//        {
//            Debug.LogWarning($"[클라이언트 수신 종료] {e.Message}");
//        }
//    }

//    public void SendSpinRequest()
//    {
//        if(myPlayerIndex == 0)
//        {
//            Debug.LogWarning("아직 방이 가득 차지 않았습니다");
//            return;
//        }

//        GamePacket spinRequest = new GamePacket { packetType = 2, player = myPlayerIndex, data = "Spin" };
//        SendPacket(spinRequest);
//    }

//    public void SendTurnEnd()
//    {
//        GamePacket packet = new GamePacket { packetType = 1, player = myPlayerIndex, data = "TurnEnd" };
//        SendPacket(packet);
//    }

//    public async void SendPacket(GamePacket packet)
//    {
//        if (stream == null) return;
//        string json = JsonUtility.ToJson(packet) + "\n";
//        byte[] data = Encoding.UTF8.GetBytes(json);
//        await stream.WriteAsync(data, 0, data.Length);
//        await stream.FlushAsync();
//    }

//    public void SendItemUse(int InventorySlotIndex)
//    {
//        GamePacket packet = new GamePacket { packetType = 3, player = myPlayerIndex, data = InventorySlotIndex.ToString() };
//        SendPacket(packet);
//    }

//    private void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
//    {
//        if(scene.name == "BattleScene")
//        {
//            SceneManager.sceneLoaded -= OnGameSceneLoaded;

//            if(PvpBattleManager.Instance != null)
//            {
//                PvpBattleManager.Instance.StartNetworkGame(savedGameSeed);
//            }
//        }
//    }

//    public void DisconnectFromServer()
//    {
//        Debug.Log("[클라이언트] 서버와의 연결을 안전하게 종료합니다.");

//        // 스트림 닫기
//        if (stream != null)
//        {
//            stream.Close();
//            stream = null;
//        }

//        // 클라이언트 닫기
//        if (client != null)
//        {
//            client.Close();
//            client = null;
//        }

//        // 로비로 돌아갔을 때를 대비한 상태 초기화
//        myPlayerIndex = 0;
//        executeOnMainThread.Clear();
//    }

//    private void OnDestroy()
//    {
//        DisconnectFromServer();
//    }
//}
