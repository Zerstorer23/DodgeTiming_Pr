using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/My Unit Config")]
public class UnitConfig : ScriptableObject
{

   public CharacterType characterID;
    public string txt_name;
    public bool noRandom = false;
    [TextArea(15, 20)] public string txt_skill_desc;    
    public Sprite portraitImage; 




    
}
