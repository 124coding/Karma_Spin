using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ShopSyncData
{
    public List<string> relicNames = new List<string>();
    public List<string> activeItemNames = new List<string>();
}

public class NetworkBattleController : NetworkBehaviour
{
    public static NetworkBattleController Instance;
    
    // 서버만 사용할 레디 카운트 변수
    private int readyCount = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            int initialSeed = UnityEngine.Random.Range(10000, 99999);
            StartGameRpc(initialSeed);

            int firstTurnSeed = UnityEngine.Random.Range(10000, 99999);
            TurnStartRpc(1, firstTurnSeed);
        }
    }

    /// ==================================================
    /// 서버가 클라이언트들에게 명령을 내리는 함수 [ClientRpc]
    /// ==================================================

    // 매칭 성공 및 씬 진입 처리
    [Rpc(SendTo.Everyone)]
    private void StartGameRpc(int seed)
    {
        // 방장은 1P, 참가자는 2P
        int myPlayerIndex = IsServer ? 1 : 2;
        Debug.Log($"매칭 성공! {myPlayerIndex}P로 게임 시작. 시드: {seed}");
    
        if(PvpBattleManager.Instance != null)
        {
            PvpBattleManager.Instance.myPlayerIndex = myPlayerIndex;
            PvpBattleManager.Instance.StartNetworkGame(seed);
        }
    }

    // 턴 종료 요청
    public void RequestTurnEnd(int myPlayerIndex)
    {
        TurnEndRpc(myPlayerIndex);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void TurnEndRpc(int playerIndex)
    {
        int nextTurnPlayer = (playerIndex == 1) ? 2 : 1;
        int resetSeed = UnityEngine.Random.Range(10000, 99999);

        TurnStartRpc(nextTurnPlayer, resetSeed);
    }

    [Rpc(SendTo.Everyone)]
    private void TurnStartRpc(int nextTurnPlayer, int resetSeed)
    {
        if(PvpBattleManager.Instance != null) PvpBattleManager.Instance.UpdateTurnState(nextTurnPlayer, resetSeed);
    }

    // 스핀 요청
    public void RequestSpin(int playerIndex)
    {
        SpinServerRpc(playerIndex);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SpinServerRpc(int playerIndex)
    {
        SpinClientRpc(playerIndex);
    }

    [Rpc(SendTo.Everyone)]
    private void SpinClientRpc(int actionPlayer)
    {
        if (PvpBattleManager.Instance != null)
            PvpBattleManager.Instance.ExecuteNetworkSpin(actionPlayer);
    }

    // 아이템 사용 요청
    public void RequestItemUse(int playerIndex, int slotIndex)
    {
        ItemUseServerRpc(playerIndex, slotIndex);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ItemUseServerRpc(int playerIndex, int slotIndex)
    {
        int reelSeed = UnityEngine.Random.Range(10000, 99999);
        ItemUseClientRpc(playerIndex, slotIndex, reelSeed);
    }

    [Rpc(SendTo.Everyone)]
    private void ItemUseClientRpc(int actionPlayer, int slotIndex, int reelSeed)
    {
        if (PvpBattleManager.Instance != null)
            PvpBattleManager.Instance.ExecuteNetworkItemUse(actionPlayer, slotIndex, reelSeed);
    }

    // 레디 완료 및 라운드 시작
    public void RequestReady(int playerIndex, string syncJsonData)
    {
        ReadyServerRpc(playerIndex, syncJsonData);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ReadyServerRpc(int playerIndex, string syncJsonData)
    {
        ReadyBroadcastRpc(playerIndex, syncJsonData);

        readyCount++;
        if (readyCount >= 2)
        {
            readyCount = 0;
            int nextRoundSeed = UnityEngine.Random.Range(10000, 99999);
            StartNextRoundRpc(nextRoundSeed);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void ReadyBroadcastRpc(int readyPlayer, string syncJsonData)
    {
        int myPlayerIndex = IsServer ? 1 : 2;
        if (readyPlayer != myPlayerIndex && PvpBattleManager.Instance != null)
        {
            PvpBattleManager.Instance.OnOpponentReady(syncJsonData);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void StartNextRoundRpc(int roundSeed)
    {
        if (PvpBattleManager.Instance != null)
            PvpBattleManager.Instance.StartNextRoundFromNetwork(roundSeed);
    }
}
