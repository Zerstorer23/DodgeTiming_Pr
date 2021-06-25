using Lex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BulletManager;
/// <summary>
/// A LexView identifies an object across the network (viewID) and configures how the controlling client updates remote instances.
/// </summary>
///     public override float Activate()
public class Action_SetProjectileScale : SkillAction
{

    public override float Activate()
    {
        parent.projectilePV.RPC("SetScale",  
            GetParam<float>(SkillParams.Width),
            GetParam<float>(SkillParams.Height));
        return 0;
    }
}

public class Action_Projectile_ToggleDamage : SkillAction
{
    // Start is called before the first frame update
    public override float Activate()
    {
        if (parent.projectilePV == null) return 0f;
        bool enable = GetParam<bool>(SkillParams.Enable);
        parent.projectilePV.RPC("ToggleDamage",   enable);
        return 0f;
    }
}

public class Action_SetProjectile_InvincibleFromMapBullets : SkillAction
{
    // Start is called before the first frame update
    public override float Activate()
    {
        if (parent.projectilePV == null) return 0f;
        bool enable = GetParam<bool>(SkillParams.Enable);
        parent.projectilePV.RPC("SetInvincibleFromMapBullets",   enable);
        return 0f;
    }
}
public class Action_DoScaleTween : SkillAction
{
    // Start is called before the first frame update
    public override float Activate()
    {
        if (parent.projectilePV == null) return 0f;
        float duration = GetParam<float>(SkillParams.Duration);
        float scale = GetParam<float>(SkillParams.Width);
        parent.projectilePV.RPC("DoTweenScale",   duration, scale);
        return 0f;
    }
}
public class Action_DoDeathAfter : SkillAction
{
    // Start is called before the first frame update
    public override float Activate()
    {
        if (parent.projectilePV == null) return 0f;
        float duration = GetParam<float>(SkillParams.Duration);
        parent.projectilePV.GetComponent<HealthPoint>().DoDeathAfter(duration);
        return 0f;
    }
}
