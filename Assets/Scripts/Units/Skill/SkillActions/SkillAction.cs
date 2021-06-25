using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillAction
{
    protected ActionSet parent;

    public SkillParams paramType;
    public object paramValue;
    public void SetSkillSet(ActionSet p) {
        parent = p;
    }
    public virtual float Activate() {
        return 0f;
    }
    public T GetParam<T>(SkillParams key) {
        return parent.GetParam<T>(key);
    }
    public T GetParam<T>(SkillParams key, object defaultVal)
    {
        return parent.GetParam<T>(key,defaultVal);
    }
}
