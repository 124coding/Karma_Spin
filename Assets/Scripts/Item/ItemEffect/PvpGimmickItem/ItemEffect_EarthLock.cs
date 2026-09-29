using UnityEngine;

[CreateAssetMenu(menuName = "Effects/EarthLock")]
public class ItemEffect_EarthLock : ItemEffect
{
    public override bool ExecuteEffect(BaseBattleManager battleManager, DamageReport report)
    {
        if(battleManager is PvpBattleManager pvpManager)
        {
            SlotManager targetSlot = pvpManager.currentItemTargetSlot;

            if (targetSlot.TryApplyGimmick(SlotGimmickState.Earth_Locked))
            {
                System.Random rng = new System.Random(pvpManager.currentItemSeed);

                // 성공했다면 잠글 릴 번호를 지정
                int targetReelIndex = rng.Next(0, 3);

                targetSlot.ApplyEarthLock(targetReelIndex);

                return true;

                // TODO: 해당 릴을 회색 돌덩이처럼 보이게 하는 셰이더나 UI 연출 적용
            }

            return false;
        }

        return false;
    }
}
