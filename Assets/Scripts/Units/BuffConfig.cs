using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/My BuffObject Config")]
public class BuffConfig : ScriptableObject
{

    public BuffType buffType;
    public float modifier;
    public float duration;
    public string triggerByID;
    public string buff_name;
    public Sprite spriteImage;

    internal BuffData Build()
    {
        return new BuffData(buffType, modifier, duration);
    }
    
}
