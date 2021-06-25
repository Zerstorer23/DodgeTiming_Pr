using Lex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageManifold 
{
    bool FindAttackHistory(int tid);
    bool CheckDuplicateDamage(int tid);
    bool RemoveAttackHistory(int tid);
    void Reset();
}
public class DamageManifold {
    public static IDamageManifold SetDamageManifold(DamageManifoldType damageManifold)
    {
        switch (damageManifold)
        {
            case DamageManifoldType.Once:
                return new DamageOnce();
            case DamageManifoldType.Queue:
                return new DamageQueue();

            case DamageManifoldType.Timed:
                return new DamageTimed();

            case DamageManifoldType.InAndOout:
                return new DamageInAndOut();

            case DamageManifoldType.None:
                return new DamageNone();

            default:
                return new DamageOnce();

        }
    }

}

public class DamageQueue : IDamageManifold
{
    Queue<int> damageRecords = new Queue<int>();
    public bool CheckDuplicateDamage(int tid)
    {
        if (damageRecords.Count > 0 && damageRecords.Peek() == tid)
        {
            return false;
        }
        else
        {
            damageRecords.Enqueue(tid);
            return true;
        }
    }

    public bool FindAttackHistory(int tid)
    {
        return damageRecords.Contains(tid);
    }

    public bool RemoveAttackHistory(int tid)
    {
        while (damageRecords.Count > 0 && damageRecords.Peek() == tid)
        {
            damageRecords.Dequeue();
        }
        return false;
    }

    public void Reset()
    {
        damageRecords.Clear();
    }
}
public class DamageOnce : IDamageManifold
{
    HashSet<int> damageRecords = new HashSet<int>();
    public bool CheckDuplicateDamage(int tid)
    {
        if (damageRecords.Contains(tid))
        {
            return false;
        }
        else
        {
            damageRecords.Add(tid);
            return true;
        }
    }

    public bool FindAttackHistory(int tid)
    {
        return damageRecords.Contains(tid);
    }

    public bool RemoveAttackHistory(int tid)
    {
        if (damageRecords.Contains(tid)) {
            damageRecords.Remove(tid);
            return true;
        }
        return false;
    }

    public void Reset()
    {
        damageRecords.Clear();
    }
}
public class DamageTimed : IDamageManifold
{
    Dictionary<int, double> damageRecords = new Dictionary<int, double>();

    public bool FindAttackHistory(int tid)
    {
        return damageRecords.ContainsKey(tid);
    }

    public bool RemoveAttackHistory(int tid)
    {
        if (damageRecords.ContainsKey(tid))
        {
            damageRecords.Remove(tid);
            return true;
        }
        return false;
    }

    bool IDamageManifold.CheckDuplicateDamage(int tid)
    {
        if (damageRecords.ContainsKey(tid))
        {
            if (LexNetwork.NetTime - damageRecords[tid] >= 0.7f)
            {
                damageRecords[tid] = LexNetwork.NetTime;
                return true;
            }
            return false;
        }
        else
        {
            damageRecords.Add(tid, LexNetwork.NetTime);
            return true;
        }
    }

    void IDamageManifold.Reset()
    {
        damageRecords.Clear();
    }
}
public class DamageInAndOut: IDamageManifold
{
    HashSet<int> damageRecords = new HashSet<int>();
    public bool FindAttackHistory(int tid)
    {
        return damageRecords.Contains(tid);
    }
    public bool CheckDuplicateDamage(int tid)
    {
        if (damageRecords.Contains(tid))
        {
            return false;
        }
        else
        {
            damageRecords.Add(tid);
            return true;
        }
    }

    public void Reset()
    {
        damageRecords.Clear();
    }

    public bool RemoveAttackHistory(int tid)
    {
        if (damageRecords.Contains(tid))
        {
            damageRecords.Remove(tid);
            return true;
        }
        return false;
    }
}

public class DamageNone : IDamageManifold
{
    List<int> damageRecords = new List<int>();
    public bool CheckDuplicateDamage(int tid)
    {
        if (!damageRecords.Contains(tid))
        {
            damageRecords.Add(tid);
        }
        return true;
    }

    public bool FindAttackHistory(int tid)
    {
        return damageRecords.Contains(tid);
    }

    public bool RemoveAttackHistory(int tid)
    {
        if (damageRecords.Contains(tid))
        {
            damageRecords.Remove(tid);
            return true;
        }
        return false;
    }

    public void Reset()
    {
        damageRecords.Clear();
    }

}

public enum DamageManifoldType
{
    Once, Queue, Timed,InAndOout,None
}