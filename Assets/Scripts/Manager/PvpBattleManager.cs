using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PvpBattleManager : BaseBattleManager
{
    public GameObject dimPanel1P;
    public GameObject dimPanel2P;

    [Header("PvP 전용 UI")]
    public SlotManager slotManager1P;
    public SlotManager slotManager2P;
    public Transform popupAnchor1P;
    public Transform popupAnchor2P;

    [Header("PvP 플레이어 데이터")]
    public PlayerManager player1;
    public PlayerManager player2;

    [Header("PvP 중앙 격돌 UI")]
    public Slider tugOfWarSlider;
    public TextMeshProUGUI pendingDamageText; // 선턴 텍스트
    public TextMeshProUGUI secondDamageText;  // 후턴 텍스트
    public Button centralSpinButton;
    public TextMeshProUGUI centralSpinText;

    [Header("PvP 룰 세팅")]
    public float currentTugGauge = 0f; // -면 1P 우세, +면 2P 우세
    public float maxTugGauge = 10000f;
    public bool isP1Turn = true;

    [Header("힘겨루기 데이터")]
    public float pendingDamage = 0f; // 선턴 플레이어가 뽑아둔 대기 데미지
    public bool isFirstSpinOfRound = true; // 현재 스핀이 라운드의 첫 번째 스핀인지

    public override SymbolType TargetSymbolType => SymbolType.None;

    public override SlotManager CurrentSlotManager => isP1Turn ? slotManager1P : slotManager2P;
    public override Transform CurrentPopupAnchor => isP1Turn ? popupAnchor1P : popupAnchor2P;

    // 턴에 따라 1P 스탯과 2P 스탯을 동적으로 스위칭
    public override float BaseDamage => isP1Turn ? player1.baseDamage : player2.baseDamage;
    public override float LineMultiplier => isP1Turn ? player1.lineMultiplier : player2.lineMultiplier;
    public override float ClusterMultiplier => isP1Turn ? player1.clusterMultiplier : player2.clusterMultiplier;
    public override float AllMultiplier => isP1Turn ? player1.allMultiplier : player2.allMultiplier;
    public override float TaegeukMultiplier => isP1Turn ? player1.taegeukMultiplier : player2.taegeukMultiplier;
    public override float BadMultiplier => isP1Turn ? player1.badMultiplier : player2.badMultiplier;

    // TODO: 테스트용 삭제 필요
    private void Start()
    {
        SetInitialize();
    }

    public override void SetInitialize()
    {
        currentTugGauge = 0f;
        isP1Turn = true;
        isFirstSpinOfRound = true;
        pendingDamage = 0f;

        if (tugOfWarSlider != null)
        {
            tugOfWarSlider.minValue = -maxTugGauge;
            tugOfWarSlider.maxValue = maxTugGauge;
            tugOfWarSlider.value = 0f;
        }

        pendingDamageText.text = ""; // 대기 데미지 숨김
        pendingDamageText.gameObject.SetActive(false);

        // 시작과 동시에 1P는 밝게, 2P는 어둡게 UI를 업데이트
        UpdateBoardState();
    }

    public void OnClickCentralSpinButton()
    {
        centralSpinButton.interactable = false;

        CurrentSlotManager.OnClickSpinButton();
    }

    public override void OnReelStopped(SymbolData[,] grid)
    {
        // 보스 기믹을 묻지 않고 순수하게 계산기 호출
        DamageReport report = DamageCalculator.CalculateTotalDamage(grid, this);
        StartCoroutine(PvpDamageRoutine(report, grid));
    }

    private IEnumerator PvpDamageRoutine(DamageReport report, SymbolData[,] grid)
    {
        Color playerColor = isP1Turn ? Color.cyan : new Color(1f, 0.4f, 0.4f);
        yield return StartCoroutine(PlayCommonDamageAnimation(report, grid, playerColor));

        // 개별 잭팟 연출

        if (isFirstSpinOfRound)
        {
            // 선턴: 데미지 킵(Keep)
            pendingDamage = report.finalDamage;
            isFirstSpinOfRound = false;

            pendingDamageText.text = pendingDamage.ToString();
            pendingDamageText.color = isP1Turn ? Color.cyan : new Color(1f, 0.4f, 0.4f);
            pendingDamageText.gameObject.SetActive(true);
            isP1Turn = !isP1Turn;
        }
        else
        {
            // 후턴: 힘겨루기 및 정산
            float secondDamage = report.finalDamage;

            secondDamageText.text = secondDamage.ToString();
            secondDamageText.color = isP1Turn ? Color.cyan : new Color(1f, 0.4f, 0.4f);
            secondDamageText.gameObject.SetActive(true);

            yield return new WaitForSeconds(1.0f);

            // 1P 데미지와 2P 데미지의 차액 계산
            float p1Damage = isP1Turn ? secondDamage : pendingDamage;
            float p2Damage = isP1Turn ? pendingDamage : secondDamage;

            float netDamage = p2Damage - p1Damage;

            // TODO: 여기서 두 텍스트가 쾅 부딪히는 애니메이션 실행

            currentTugGauge += netDamage;

            if (tugOfWarSlider != null) tugOfWarSlider.value = currentTugGauge;

            yield return new WaitForSeconds(0.5f);
            pendingDamageText.gameObject.SetActive(false);
            secondDamageText.gameObject.SetActive(false);

            isFirstSpinOfRound = true; // 라운드 초기화
            isP1Turn = !isP1Turn;
            currentTurns--; // 두 명이 모두 쳤으므로 턴 차감
        }

        UpdateBoardState();

        if (slotManager1P != null) slotManager1P.UnlockSpinButton();
        if (slotManager2P != null) slotManager2P.UnlockSpinButton();

        centralSpinButton.interactable = true;
    }

    public void UpdateBoardState()
    {
        // 대기 중인 슬롯은 어둡게(Dim) 처리
        if (dimPanel1P != null) dimPanel1P.SetActive(!isP1Turn);
        if (dimPanel2P != null) dimPanel2P.SetActive(isP1Turn);

        centralSpinButton.image.color = isP1Turn ? Color.cyan : new Color(1f, 0.4f, 0.4f); // 1P 파랑, 2P 빨강
        centralSpinText.text = isP1Turn ? "1P SPIN" : "2P SPIN";
        centralSpinButton.interactable = true;
    }
}
