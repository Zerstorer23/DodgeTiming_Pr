using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantStrings : MonoBehaviour
{

    public const string PREFAB_BULLET_1 = "Prefabs/Projectiles/Bullet1";
    public const string PREFAB_BULLET_DESOLATION = "Prefabs/Projectiles/Bullet_desolation";
    public const string PREFAB_BULLET_ASAKURA = "Prefabs/Projectiles/Bullet_asakura";
    public const string PREFAB_BULLET_T = "Prefabs/Projectiles/Bullet_T";
    public const string PREFAB_BULLET_TSURUYA = "Prefabs/Projectiles/Bullet_tsuruya";
    public const string PREFAB_BULLET_KIMIDORI = "Prefabs/Projectiles/Bullet_kimidori";
    public const string PREFAB_BULLET_NAGATO = "Prefabs/Projectiles/Bullet_nagato";
    public const string PREFAB_BULLET_SASAKI = "Prefabs/Projectiles/Bullet_sasaki";
    public const string PREFAB_BULLET_HARUHI = "Prefabs/Projectiles/Bullet_haruhi";
    public const string PREFAB_BULLET_MORI = "Prefabs/Projectiles/Bullet_mori";
    public const string PREFAB_BULLET_MIKURU = "Prefabs/Projectiles/Bullet_mikuru";
    public const string PREFAB_BULLET_KOIZUMI = "Prefabs/Projectiles/Bullet_koizumi";
    public const string PREFAB_BULLET_KYOUKO = "Prefabs/Projectiles/Bullet_kyouko";
    public const string PREFAB_BULLET_KUYOU = "Prefabs/Projectiles/Bullet_kuyou";
    public const string PREFAB_BULLET_KYONMOUTO = "Prefabs/Projectiles/Bullet_kyonmouto";
    public const string PREFAB_BULLET_KYONKO = "Prefabs/Projectiles/Bullet_kyonko";
    public const string PREFAB_BULLET_Body = "Prefabs/Projectiles/Bullet_Body";



    public const string PREFAB_HEAL_1 = "Prefabs/Units/HealParticle";
    public const string PREFAB_EXPLOSION_1 = "Prefabs/Units/ExplosionParticle";
    public const string PREFAB_PLAYER = "Prefabs/Units/Player";
    public const string PREFAB_DESOLATOR = "Prefabs/Units/Desolator";
    public const string PREFAB_BUFF_OBJECT = "Prefabs/Units/BuffObject";
    public const string PREFAB_STARTSCENE_PLAYERNAME = "Prefabs/ConnectedUserName";
    public const string PREFAB_STARTSCENE_BOTNAME = "Prefabs/BotUserName";
    public const string TAG_PROJECTILE = "Projectile";
    public const string TAG_PLAYER = "Player";
    public const string TAG_BOUNDARY = "MapBoundary";
    public const string TAG_BOX_OBSTACLE = "BoxObstacle";
    public const string TAG_BUFF_OBJECT = "BuffObject";
    public const string TAG_WALL = "Wall";
    public const string TAG_CONVEYER_BELT = "ConveryerBelt";
    public const string TAG_TELEPORTER = "Teleporter";

    public const string PREFS_MUTED = "PrefsMuted";
    public const string PREFS_KILLS = "myTotalKills";
    public const string PREFS_EVADES = "myTotalEvades";
    public const string PREFS_WINS = "myTotalWins";
    public const string PREFS_MY_PAD = "myPad";
    public const string PREFS_FORCE_WASD = "forceWASD";
    public const string PREFS_MANUAL_AIM = "manualAim";
    public const string PREFS_TIME_RECORD = "timeRecord";

    public static string[] team_color = { "#C1C1C1", "#0080FF", "#FF8000" };
    public static string[] team_name = {"중립", "파랑", "주황" };

    private static Dictionary<string, Color> colorCache = new Dictionary<string, Color>();
    public static Color GetColorByHex(string hex)
    {
        if (colorCache.ContainsKey(hex)) {
            return colorCache[hex];
        }
        ColorUtility.TryParseHtmlString(hex, out Color newCol);
        colorCache.Add(hex,newCol);
        return newCol;
    }
    public static float GetAngleBetween(Vector3 from, Vector3 to)
    {
        Vector3 diff = to - from;
        diff.Normalize();
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        if (rot_z < 0) {
            rot_z = rot_z+360;
        }
       // Debug.Log(from + " - " + to + " = " + rot_z);
        return rot_z;// new Vector3(0f, 0f, rot_z);
    }
    public static Vector3 GetAngledVector(float eulerAngle, float distance) {
        float rad = eulerAngle / 180 * Mathf.PI;
        float dX = Mathf.Cos(rad) * distance;
        float dY = Mathf.Sin(rad) * distance;
        return new Vector3(dX, dY);
    }

    internal static float GetRadius(Vector3 localScale)
    {
        return (Mathf.Max(localScale.x, localScale.y) * 0.5f);
    }
}

public enum GameMode { 
    PVP,TEAM, Tournament, PVE,TeamCP

}
public enum Team { 
    NONE,HOME,AWAY
}