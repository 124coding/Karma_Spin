using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Metal_Shield")]
public class ItemEffect_MetalShield : ItemEffect
{
    [Header("쉴드 제공량")]
    public float shieldAmount = 100f;

    public override bool ExecuteEffect(BaseBattleManager battleManager, DamageReport report)
    {
        if(battleManager is PvpBattleManager pvpManager)
        {
            PlayerManager myPlayer = pvpManager.currentItemCaster;

            myPlayer.currentShieldHP += shieldAmount;

            // TODO: 줄다리기 UI 족에 아이콘과 수치 업데이트

            return true;
        }

        return false;
    }
}
