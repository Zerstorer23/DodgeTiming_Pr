public class Action_Player_AddBuff : SkillAction
{
    public override float Activate()
    {
        BuffData buff = GetParam<BuffData>(SkillParams.BuffData);
        parent.caster.GetComponent<BuffManager>().pv.RPC("AddBuff",   (int)buff.buffType, buff.modifier, buff.duration);
        return 0f;
    }
}public class Action_Player_MovespeedBuff : SkillAction
{
    public override float Activate()
    {
        float mod = GetParam<float>(SkillParams.Modifier);
        float duration = GetParam<float>(SkillParams.Duration);
        BuffType buffType = BuffType.MoveSpeed;
        parent.casterPV.RPC("AddBuff",   (int)buffType, mod, (double)duration);
        return 0f;
    }
}
public class Action_Player_InvincibleBuff : SkillAction
{
    public override float Activate()
    {
        if (GameFieldManager.CheckSuddenDeathCalled(parent.castingPlayer.fieldNo)) return 0f;
        if (GameSession.gameModeInfo.gameMode == GameMode.Tournament) return 0f;
        float duration =GetParam<float>(SkillParams.Duration);
        parent.casterPV.RPC("AddBuff",   (int)BuffType.InvincibleFromBullets, 0f, (double)duration);
        return 0f;
    }
}