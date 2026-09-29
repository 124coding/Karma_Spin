using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class MatchmakingManager : MonoBehaviour
{
    public static MatchmakingManager Instance;

    // 참여코드
    public string JoinCode { get; private set; }

    private string currentLobbyId;
    private bool isHeartbeating;

    // UI에게 상태를 알려줄 이벤트 선언
    public event Action<string> OnMatchStateChanged;
    public event Action OnMatchSuccess;
    public event Action OnMatchFailed;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandlePlayerDisconnect;
    }

    // 매칭 시작
    public async void StartAutoMatchmaking()
    {
        Debug.Log("매칭 시작...");

        // 1차 검색
        bool joinedSuccessfully = await TryQuickJoin();

        if (!joinedSuccessfully)
        {
            Debug.Log("방이 없습니다. 클라우드 갱신 대기 후 다시 검색합니다 (2초)...");
            OnMatchStateChanged?.Invoke("방 찾는 중...");

            // 1번 창이 방을 만드는 시간을 기다려줌
            await Task.Delay(2000);

            // 2차 검색
            joinedSuccessfully = await TryQuickJoin();

            // 그래도 없으면 진짜 방장으로 생성
            if (!joinedSuccessfully)
            {
                Debug.Log("여전히 방이 없습니다. 방장(Host)으로 방을 생성합니다.");
                await CreateRoomAndWait();
            }
        }
    }

    // 15초마다 로비 유지 핑(Ping)을 보내는 루프
    private async void KeepLobbyAliveAsync()
    {
        isHeartbeating = true;
        while (isHeartbeating)
        {
            await Task.Delay(15000); // 15초 대기
            if (!isHeartbeating) break;

            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(currentLobbyId);
                Debug.Log("로비 유지 관리(Heartbeat) 전송 성공");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning($"하트비트 전송 실패 (방이 이미 닫혔을 수 있습니다): {e.Message}");
                isHeartbeating = false;
            }
        }
    }

    private async Task<bool> TryQuickJoin()
    {
        try
        {
            // 로비 서비스의 빠른 참가 호출
            Lobby joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            // 로비에 등록된 릴레이 참여 코드 추출
            string relayJoinCode = joinedLobby.Data["RelayJoinCode"].Value;

            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            var udpEndpoint = joinAllocation.ServerEndpoints.First(e => e.ConnectionType == "udp");

            var unityTransport = Unity.Netcode.NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            var relayServerData = new Unity.Networking.Transport.Relay.RelayServerData(
                udpEndpoint.Host,
                (ushort)udpEndpoint.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.ConnectionData,     // 4번째: 참가자의 데이터
                joinAllocation.HostConnectionData, // 5번째: 방장의 데이터
                joinAllocation.Key,
                false
            );

            unityTransport.SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

            Debug.Log("빈 방을 찾아 참가자(Client)로 접속 시도!");
            return true;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning($"빠른 참가 실패: {e}");
            return false;
        }
    }

    private async Task CreateRoomAndWait()
    {
        try
        {
            // 릴레이 방 먼저 생성
            var allocation = await RelayService.Instance.CreateAllocationAsync(1);
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // 로비 생성
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                Data = new System.Collections.Generic.Dictionary<string, DataObject>
                {
                    {"RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode) }
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync("PvPRoom", 2, options);

            currentLobbyId = lobby.Id;
            KeepLobbyAliveAsync();

            // 호스트로 가동
            var relayServerData = new Unity.Networking.Transport.Relay.RelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.ConnectionData,
                allocation.ConnectionData,
                allocation.Key,
                false
            );

            var unityTransport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            unityTransport.SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            Debug.Log("새로운 방을 파서 방장(Host)으로 대기합니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"방 생성 실패: {e.Message}");
        }
    }
    
    // 매칭 성공
    private void HandleClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if(clientId == NetworkManager.Singleton.LocalClientId)
            {
                OnMatchStateChanged?.Invoke("상대방을 기다리는 중..");
            }
            else
            {
                Debug.Log("게임 시작");
                OnMatchStateChanged?.Invoke("게임 시작");
                OnMatchSuccess?.Invoke();

                // 매칭 성사로 로비 목록에서 방 삭제
                isHeartbeating = false;
                if (!string.IsNullOrEmpty(currentLobbyId))
                {
                    LobbyService.Instance.DeleteLobbyAsync(currentLobbyId);
                    currentLobbyId = null;
                }

                NetworkManager.Singleton.SceneManager.LoadScene("BattleScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }
        else
        {
            OnMatchStateChanged?.Invoke("성공적으로 접속");
            OnMatchSuccess?.Invoke();
        }
    }

    // 누군가 연결이 끊겼을 때 유니티 엔진이 자동으로 호출해 주는 콜백
    private void HandlePlayerDisconnect(ulong clientId)
    {
        // 내가 로비화면에서 접속 시도하다가 실패한 경우 (매칭 실패)
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "LobbyScene")
        {
            OnMatchFailed?.Invoke();
            return;
        }

        // 게임 도중 끊긴 경우
        if (NetworkManager.Singleton.IsServer)
        {
            if (clientId != NetworkManager.Singleton.LocalClientId)
            {
                if (PvpBattleManager.Instance != null) PvpBattleManager.Instance.HandleOpponentDisconnected();
            }
        }
        else
        {
            if (PvpBattleManager.Instance != null) PvpBattleManager.Instance.HandleOpponentDisconnected();
        }
    }

    // [비공개 방 만들기] 버튼 연결용
    public async Task CreatePrivateRoom()
    {
        try
        {
            Debug.Log("비공개 방을 생성합니다...");

            // 로비(Lobby) 없이 릴레이(Relay) 방만 생성
            var allocation = await RelayService.Instance.CreateAllocationAsync(1);

            JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var udpEndpoint = allocation.ServerEndpoints.First(e => e.ConnectionType == "udp");

            var relayServerData = new Unity.Networking.Transport.Relay.RelayServerData(
                udpEndpoint.Host,
                (ushort)udpEndpoint.Port,
                allocation.AllocationIdBytes,
                allocation.ConnectionData,
                allocation.ConnectionData,
                allocation.Key,
                false
            );

            var unityTransport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            unityTransport.SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            Debug.Log($"<color=green>비공개 방 생성 완료! 참여 코드: {JoinCode}</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"비공개 방 생성 실패: {e.Message}");
        }
    }

    // [코드 입력 후 참여] 버튼 연결용
    public async void JoinPrivateRoom(string inputJoinCode)
    {
        try
        {
            Debug.Log($"코드 [{inputJoinCode}] (으)로 비공개 방 접속 시도 중...");

            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(inputJoinCode);

            var unityTransport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            var relayServerData = new Unity.Networking.Transport.Relay.RelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData,
                joinAllocation.Key,
                false
            );
            unityTransport.SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();

            Debug.Log("<color=green>비공개 방 접속 성공!</color>");
        }
        catch (RelayServiceException e)
        {
            Debug.LogWarning($"접속 실패! 코드를 다시 확인해주세요: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        isHeartbeating = false;

        // 씬이 넘어갈 때 이벤트 구독 해제 (메모리 누수 방지)
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandlePlayerDisconnect;
        }
    }

    private async void OnApplicationQuit()
    {
        if (!string.IsNullOrEmpty(currentLobbyId))
        {
            try
            {
                await Unity.Services.Lobbies.LobbyService.Instance.DeleteLobbyAsync(currentLobbyId);
                Debug.Log("클라우드 로비 청소 완료");
            }
            catch (System.Exception)
            {
                // 이미 삭제된 방이면 무시
            }
        }
    }
}
