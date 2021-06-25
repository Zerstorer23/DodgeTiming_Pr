
using System;

using System.Collections;

using System.Collections.Generic;

using UnityEngine;



[System.Serializable]

public class BuffList<T> : IList

{

    [SerializeField]
    List<T> m_List;


    public T this[int index] { get => m_List[index]; set => m_List[index] = value; }

    public bool IsFixedSize
    {
        get
        {
            return true;
        }
    }
    public bool IsReadOnly
    {
        get
        {
            return false;
        }
    }

    public int Add(T value)
    {
        m_List.Add((T)value);
        return m_List.Count;
    }

    public void Clear()
    {
        m_List = new List<T>();
    }

    public bool Contains(T value)
    {
        return m_List.Contains(value);
    }

    public int IndexOf(T value)
    {
        return m_List.IndexOf(value);
    }

    public void Insert(int index, T value)
    {
        m_List.Insert(index, value);
    }

    public void RemoveAt(int index)
    {
        m_List.RemoveAt(index);
    }

    // ICollection members.


    public int Count
    {
        get
        {
            return m_List.Count;
        }
    }

    public bool IsSynchronized => ((ICollection)m_List).IsSynchronized;

    public object SyncRoot => ((ICollection)m_List).SyncRoot;

    object IList.this[int index] { get => ((IList)m_List)[index]; set => ((IList)m_List)[index] = value; }


    // IEnumerable Members

    public IEnumerator GetEnumerator()
    {
        // Refer to the IEnumerator documentation for an example of
        // implementing an enumerator.
        throw new NotImplementedException("The method or operation is not implemented.");
    }

    public void PrintContents()
    {
        Console.WriteLine($"List currently has {m_List.Count} elements.");
        Console.Write("List contents:");
        for (int i = 0; i < Count; i++)
        {
            Console.Write($" {m_List[i]}");
        }
        Console.WriteLine();
    }



    public void CopyTo(T[] array, int arrayIndex)
    {
        ((ICollection<T>)m_List).CopyTo(array, arrayIndex);
    }


    public int Add(object value)
    {
        return ((IList)m_List).Add(value);
    }

    public bool Contains(object value)
    {
        return ((IList)m_List).Contains(value);
    }

    public int IndexOf(object value)
    {
        return ((IList)m_List).IndexOf(value);
    }

    public void Insert(int index, object value)
    {
        ((IList)m_List).Insert(index, value);
    }

    public void Remove(object value)
    {
        ((IList)m_List).Remove(value);
    }

    public void CopyTo(Array array, int index)
    {
        ((ICollection)m_List).CopyTo(array, index);
    }
}


