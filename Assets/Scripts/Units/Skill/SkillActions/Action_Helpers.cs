using Lex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_SetParameter : SkillAction
{

    public override float Activate()
    {
        parent.SetParam(paramType, paramValue);
        return 0f;
    }
}
public class Action_ModifyParameter_Vector3Multiply : SkillAction
{

    public override float Activate()
    {
        Vector3 value = parent.GetParam<Vector3>(paramType);
        value *= (float)paramValue;
        parent.SetParam(paramType, value);
        return 0f;
    }
}
public class Action_ModifyParameter_floatMultiply : SkillAction
{

    public override float Activate()
    {
        float value = parent.GetParam<float>(paramType);
        value *= (float)paramValue;
        parent.SetParam(paramType, value);
        return 0f;
    }
}

public class Action_SetAngle : SkillAction
{

    public override float Activate()
    {
        parent.SetParam(SkillParams.EulerAngle, paramValue);
        parent.SetParam(SkillParams.Quarternion, Quaternion.Euler(0, 0, (float)paramValue));
        return 0f;
    }
}
public class Action_ModifyAngle : SkillAction
{

    public override float Activate()
    {
        float angle = GetParam<float>(SkillParams.EulerAngle);
        angle += (float)paramValue;
        parent.SetParam(SkillParams.EulerAngle, angle);
        parent.SetParam(SkillParams.Quarternion, Quaternion.Euler(0, 0, (float)angle));
        return 0f;
    }
}
public class Action_WaitForSeconds : SkillAction
{
    public override float Activate()
    {
        float duration = GetParam<float>(SkillParams.Duration);
        return duration;
    }
}