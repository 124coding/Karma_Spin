using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Attribute_Boost")]
public class ItemEffect_AttributeBoost : ItemEffect
{
    [Header("증폭 설정")]
    public SymbolType targetAttribute;
    public float boostRate = 0.5f;

    public override bool ExecuteEffect(BaseBattleManager battleManager, DamageReport report)
    {
        if(battleManager is PvpBattleManager pvpManager)
        {
            PlayerManager myPlayer = pvpManager.CurrentPlayer;

            myPlayer.AddTempSymbolMultiplier(targetAttribute, boostRate);

            // TODO: UI에 공격력 증가 버프 이펙트
            return true;
        }

        return false;
    }
}
