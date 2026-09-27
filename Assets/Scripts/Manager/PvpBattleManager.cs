using System.Collections;
using TMPro;
using Unity.Multiplayer.PlayMode;
using UnityEngine;
using UnityEngine.UI;

public class PvpBattleManager : BaseBattleManager
{
    [Header("시스템 연결")]
    public PvpShopManager shopManager;
    public PvpUIManager uiManager;

    [Header("상점 및 레디 상태")]
    public GameObject opponentReadyUI;

    public void OnOpponentReady(string syncJsonData)
    {
        Debug.Log("상대방 준비 완료");
        if(opponentReadyUI != null) opponentReadyUI.SetActive(true);

        ShopSyncData syncData = JsonUtility.FromJson<ShopSyncData>(syncJsonData);

        // 유물 중복 장착(OnEquip 뻥튀기) 차단
        foreach (string relicName in syncData.relicNames)
        {
            // 내 컴퓨터의 2P 데이터에 아직 없는 유물만 추가
            if (!player2.relics.Exists(r => r.relicName == relicName))
            {
                Relic foundRelic = shopManager.allRelicItems.Find(r => r.relicName == relicName);
                if (foundRelic != null)
                {
                    // AddRelic이 호출되면서 OnEquip은 획득 시점에 딱 한 번만 실행
                    player2.AddRelic(foundRelic, slotManager2P);
                }
            }
        }

        player2.activeInventory.Clear();

        foreach(string itemName in syncData.activeItemNames)
        {
            ItemData foundItem = shopManager.allActiveItems.Find(i => i.itemName == itemName);
            if (foundItem != null)
            {
                player2.activeInventory.Add(foundItem);
            }
        }
    }

    public void StartNextRoundFromNetwork(int roundSeed)
    {
        Debug.Log("서버 명령 수신: 상점을 닫고 다음 라운드를 시작합니다.");

        // 상점 UI와 레디 표시기 닫기
        shopManager.CloseShop();
        if (opponentReadyUI != null) opponentReadyUI.SetActive(false);

        SetInitialize();

        System.Random roundDice = new System.Random(roundSeed);
        int seedFor1P = roundDice.Next();
        int seedFor2P = roundDice.Next();

        if (NetworkTest.Instance.myPlayerIndex == 1)
        {
            slotManager1P.SettingReels(seedFor1P);
            slotManager2P.SettingReels(seedFor2P);
        }
        else
        {
            slotManager1P.SettingReels(seedFor2P);
            slotManager2P.SettingReels(seedFor1P);
        }
    }


    [Header("PvP 전용 UI")]
    public SlotManager slotManager1P;
    public SlotManager slotManager2P;
    public Transform popupAnchor1P;
    public Transform popupAnchor2P;

    [Header("PvP 플레이어 데이터")]
    public PlayerManager player1;
    public PlayerManager player2;

    private bool isMyTurn = false;

    public bool IsMyTurn => isMyTurn;

    [Header("PvP 룰 세팅")]
    public float currentTugGauge = 0f; // -면 1P 우세, +면 2P 우세
    public float maxTugGauge = 10000f;

    [Header("힘겨루기 데이터")]
    public float pendingDamage = 0f; // 선턴 플레이어가 뽑아둔 대기 데미지
    public bool isFirstSpinOfRound = true; // 현재 스핀이 라운드의 첫 번째 스핀인지

    [Header("라운드 스코어")]
    public int p1WinCount = 0;
    public int p2WinCount = 0;
    public int targetWins = 2; // 3판 2선승제

    [Header("결과")]
    public int haveGold = 1000;

    public override SymbolType TargetSymbolType => SymbolType.None;

    // 턴에 따라 1P 스탯과 2P 스탯을 동적으로 스위칭
    public override SlotManager CurrentSlotManager => isMyTurn ? slotManager1P : slotManager2P;
    public PlayerManager CurrentPlayer => isMyTurn ? player1 : player2;
    public override Transform CurrentPopupAnchor => isMyTurn ? popupAnchor1P : popupAnchor2P;
    public override float BaseDamage => isMyTurn ? player1.baseDamage : player2.baseDamage;
    public override float LineMultiplier => isMyTurn ? player1.lineMultiplier : player2.lineMultiplier;
    public override float ClusterMultiplier => isMyTurn ? player1.clusterMultiplier : player2.clusterMultiplier;
    public override float AllMultiplier => isMyTurn ? player1.allMultiplier : player2.allMultiplier;
    public override float TaegeukMultiplier => isMyTurn ? player1.taegeukMultiplier : player2.taegeukMultiplier;

    public static PvpBattleManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void StartNetworkGame(int gameSeed)
    {
        System.Random initDice = new System.Random(gameSeed);

        int seedFor1P = initDice.Next();
        int seedFor2P = initDice.Next();

        if (NetworkTest.Instance.myPlayerIndex == 1)
        {
            // 내가 1P라면: 왼쪽이 1P 시드, 오른쪽이 2P 시드
            ResetFullGame(seedFor1P, seedFor2P);
        }
        else
        {
            // 내가 2P라면: 왼쪽이 2P 시드, 오른쪽이 1P 시드
            ResetFullGame(seedFor2P, seedFor1P);
        }
    }

    private void Start()
    {
        player1.OnGoldChanged += uiManager.SetGoldText;

        player1.OnShieldChanged += uiManager.SetP1ShieldText;
        player2.OnShieldChanged += uiManager.SetP2ShieldText;
    }

    private void OnDestroy()
    {
        player1.OnGoldChanged -= uiManager.SetGoldText;

        player1.OnShieldChanged -= uiManager.SetP1ShieldText;
        player2.OnShieldChanged -= uiManager.SetP2ShieldText;
    }

    public override void SetInitialize()
    {
        currentTurns = baseTurnLimit;
        currentTugGauge = 0f;
        isMyTurn = false;
        isFirstSpinOfRound = true;
        pendingDamage = 0f;

        slotManager1P.ClearGimmick();
        slotManager2P.ClearGimmick();

        player1.ResetTurnData();
        player2.ResetTurnData();
        player1.currentShieldHP = 0f;
        player2.currentShieldHP = 0f;

        player1.TriggerBattleStart(slotManager1P);
        player2.TriggerBattleStart(slotManager2P);

        uiManager.RefreshAllInventoryUI();
        uiManager.UpdateTugGaugeText(currentTugGauge);

        uiManager.UpdateBoardDimState(true);
        uiManager.SetSpinButtonInteractable(true);

    }

    [HideInInspector] public int currentTurnSeed;

    public void UpdateTurnState(int currentTurnPlayer, int resetSeed)
    {
        currentTurnSeed = resetSeed;
        isMyTurn = (currentTurnPlayer == NetworkTest.Instance.myPlayerIndex);

        if (currentTurnPlayer == 1)
        {
            player1.ResetTurnData();
        }
        else
        {
            player2.ResetTurnData();
        }


        if (isMyTurn)
        {
            Debug.Log("나의 턴");
            uiManager.SetSpinButtonInteractable(true);
            uiManager.UpdateBoardDimState(true);
        }
        else
        {
            Debug.Log("상대 턴");
            uiManager.SetSpinButtonInteractable(false);
            uiManager.UpdateBoardDimState(false);
        }
    }

    public void OnClickSpinButton()
    {
        if (!isMyTurn) return;
        uiManager.SetSpinButtonInteractable(false);

        if (NetworkTest.Instance != null)
        {
            NetworkTest.Instance.SendSpinRequest();
        }
        else
        {
            Debug.LogError("네트워크 매니저가 없습니다.");
        }
    }

    public void ExecuteNetworkSpin(int actionPlayerIndex)
    {
        Debug.Log("서버의 스핀 명령 수신! 양쪽 릴을 동시에 회전시킵니다.");

        if (actionPlayerIndex == NetworkTest.Instance.myPlayerIndex)
        {
            slotManager1P.ExecuteNetworkSpinSequence();
        }
        // 상대방이라면 오른쪽 릴(p2)을 돌림
        else
        {
            slotManager2P.ExecuteNetworkSpinSequence();
        }
    }

    [HideInInspector] public SlotManager currentItemCasterSlot;
    [HideInInspector] public SlotManager currentItemTargetSlot;
    [HideInInspector] public int currentItemSeed;

    [HideInInspector] public PlayerManager currentItemCaster;
    [HideInInspector] public PlayerManager currentItemTarget;

    public void ExecuteNetworkItemUse(int actionPlayerIndex, int slotIndex, int reelSeed)
    {
        bool didIUse = (actionPlayerIndex == NetworkTest.Instance.myPlayerIndex);
        PlayerManager userPlayer = didIUse ? player1 : player2;

        currentItemCaster = didIUse ? player1 : player2;
        currentItemTarget = didIUse ? player2 : player1;

        currentItemCasterSlot = didIUse ? slotManager1P : slotManager2P;
        currentItemTargetSlot = didIUse ? slotManager2P : slotManager1P;
        currentItemSeed = reelSeed;

        if (slotIndex >= 0 && slotIndex < userPlayer.activeInventory.Count)
        {
            ItemData usedItem = userPlayer.activeInventory[slotIndex];

            // 동시에 아이템 효과 발동
            usedItem.UseItem(this, null);

            // 아이템 소모 처리
            userPlayer.activeInventory.RemoveAt(slotIndex);

            // UI 갱신
            if (didIUse) uiManager.RefreshAllInventoryUI();

            Debug.Log($"{(didIUse ? "내" : "상대방")}이(가) {usedItem.itemName} 아이템을 사용했습니다!");
        }
    }

    public override void OnReelStopped(SymbolData[,] grid)
    {
        // 보스 기믹을 묻지 않고 순수하게 계산기 호출
        DamageReport report = DamageCalculator.CalculateTotalDamage(grid, this);

        bool wasMyTurn = isMyTurn;

        StartCoroutine(PvpDamageRoutine(report, grid, wasMyTurn));
    }

    private IEnumerator PvpDamageRoutine(DamageReport report, SymbolData[,] grid, bool wasMyTurn)
    {
        Color playerColor = isMyTurn ? Color.cyan : new Color(1f, 0.4f, 0.4f);
        yield return StartCoroutine(PlayCommonDamageAnimation(report, grid, playerColor));

        if (isFirstSpinOfRound)
        {
            ProcessFirstTurn(report.finalDamage);
        }
        else
        {
            yield return StartCoroutine(ProcessClashRoutine(report.finalDamage));
        }

        HandleTurnEnd(wasMyTurn);
    }

    // 선턴 처리
    private void ProcessFirstTurn(float damage)
    {
        pendingDamage = damage;
        isFirstSpinOfRound = false;

        uiManager.ShowPendingDamage(pendingDamage, isMyTurn);
    }

    private IEnumerator ProcessClashRoutine(float secondDamage)
    {
        uiManager.ShowSecondDamage(secondDamage, isMyTurn);

        yield return new WaitForSeconds(1.0f);

        // 데미지 차액 계산
        float myDamage = isMyTurn ? secondDamage : pendingDamage;
        float enemyDamage = isMyTurn ? pendingDamage : secondDamage;

        float rawNetDamage = enemyDamage - myDamage;
        float finalNetDamage = 0f;

        if (rawNetDamage > 0)
        {
            float damageToP1 = CalculateDamageThroughShield(rawNetDamage, player1);
            finalNetDamage = damageToP1;
        }
        else if (rawNetDamage < 0)
        {
            float damageToP2 = CalculateDamageThroughShield(Mathf.Abs(rawNetDamage), player2);
            finalNetDamage = -damageToP2;
        }
        else finalNetDamage = 0f;

        // TODO: 두 데미지 부딪히는 애니메이션 실행

        currentTugGauge += finalNetDamage;
        uiManager.UpdateTugGaugeText(currentTugGauge);

        yield return new WaitForSeconds(0.5f);

        uiManager.HideDamageTexts();

        isFirstSpinOfRound = true;
        currentTurns--;
    }

    private float CalculateDamageThroughShield(float damage, PlayerManager defender)
    {
        if (defender.currentShieldHP <= 0) return damage;

        if(defender.currentShieldHP >= damage)
        {
            defender.currentShieldHP -= damage;
            return 0f;
        }
        else
        {
            defender.currentShieldHP = 0f;
            return damage;
        }
    }

    public override float GetTempSymbolMultiplier(SymbolType type)
    {
        PlayerManager currentPlayer = isMyTurn ? player1 : player2;

        if (currentPlayer.tempSymbolMultipliers.TryGetValue(type, out float bonus)) return 1f + bonus;

        return 1f;
    }

    private void HandleTurnEnd(bool wasMyTurn)
    {
        PlayerManager spinner = wasMyTurn ? player1 : player2;
        SlotManager spinnerSlot = wasMyTurn ? slotManager1P : slotManager2P;

        spinner.tempSymbolMultipliers.Clear();
        spinnerSlot.ResetTempReelBuffs(currentTurnSeed);

        // 기믹 턴 종료 처리 (방금 스핀을 마친 내 보드판 기준)
        if (spinnerSlot.currentState == SlotGimmickState.Earth_Locked)
        {
            spinnerSlot.ClearGimmick();
        }
        if (spinnerSlot != null)
        {
            spinnerSlot.SpreadCorruption();
        }

        // --------------------------------------------------------
        // 턴 넘김 및 다음 사람을 위한 UI 셋업
        // --------------------------------------------------------
        uiManager.RefreshAllInventoryUI();

        if (slotManager1P != null) slotManager1P.UnlockSpinButton();
        if (slotManager2P != null) slotManager2P.UnlockSpinButton();

        if (currentTurns <= 0)
        {
            if (currentTugGauge < 0)
            {
                Debug.Log("1P 라운드 승리!");
                p1WinCount++;
            }
            else if (currentTugGauge > 0)
            {
                Debug.Log("2P 라운드 승리!");
                p2WinCount++;
            }
            else
            {
                Debug.Log("무승부");
            }

            uiManager.SetP1WinCountText(p1WinCount);
            uiManager.SetP2WinCountText(p2WinCount);

            player1.gold += haveGold;
            player2.gold += haveGold;

            // TODO: 오버킬(피냐타) 데미지 환산 골드 보너스 추가 로직 자리

            if (p1WinCount >= targetWins || p2WinCount >= targetWins)
            {
                bool isVictory = (p1WinCount >= targetWins);
                uiManager.ShowFinalResult(isVictory);
            }
            else
            {
                shopManager.OpenShop(this);
            }
        }
        else
        {
            if (wasMyTurn && NetworkTest.Instance != null)
            {
                NetworkTest.Instance.SendTurnEnd();
            }
        }
    }

    public void ResetFullGame(int mySeed, int enemySeed)
    {
        // 스코어 및 재화 초기화
        p1WinCount = 0;
        p2WinCount = 0;
        
        uiManager.SetP1WinCountText(p1WinCount);
        uiManager.SetP2WinCountText(p2WinCount);

        player1.gold = 0;
        player2.gold = 0;

        player1.activeInventory.Clear();
        player1.relics.Clear();

        player2.activeInventory.Clear();
        player2.relics.Clear();

        UnityEngine.Random.InitState(mySeed);
        slotManager1P.InitializeNetworkGame(mySeed);

        UnityEngine.Random.InitState(enemySeed);
        slotManager2P.InitializeNetworkGame(enemySeed);

        uiManager.HideFinalResults();

        Debug.Log("게임을 처음부터 다시 시작합니다!");
        SetInitialize(); // 1라운드 셋업 호출
    }

    public void HandleOpponentDisconnected()
    {
        Debug.LogWarning("[시스템] 상대 탈주 감지.");

        StopAllCoroutines();

        if (shopManager != null) shopManager.CloseShop();

        if(uiManager != null)
        {
            uiManager.ShowOpponentDisconnectedUI();
        }
    }
}
