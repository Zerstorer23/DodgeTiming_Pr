using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ISkill
{
    public abstract ActionSet GetSkillActionSet(SkillManager skm);
    public abstract void LoadInformation(SkillManager skm);

    public virtual void OnPlayerKilledPlayer(EventObject eo) { 
    
    }
    public virtual void OnMyProjectileHit(EventObject eo)
    {

    }
    public virtual void OnMyProjectileMiss(EventObject eo)
    {

    }
}
