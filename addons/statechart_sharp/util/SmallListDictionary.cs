using System;
using System.Collections.Generic;


namespace LGWCP.Util;

/// <summary>
/// Small insert-only dictionary, implemented with list. No check for key or value.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TVal"></typeparam>
public class SmallListDictionary<TKey, TVal>
    where TKey : IEquatable<TKey>
{
    protected List<TKey> Keys = new();
    protected List<TVal> Vals = new();

    /// <summary>
    /// Try insert kv. Fail if key collides.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val"></param>
    /// <returns>Returns inserted kv index, -1 if insert failed.</returns>
    public int TryInsert(TKey key, TVal val)
    {
        for (int i = 0; i < Keys.Count; ++i)
        {
            if (Keys[i].Equals(key))
            {
                return -1;
            }
        }

        Keys.Add(key);
        Vals.Add(val);
        return Keys.Count;
    }

    /// <summary>
    /// Try insert kv. Replace value if key collides.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val"></param>
    /// <returns>Returns inserted kv index.</returns>
    public int TryInsertOrReplace(TKey key, TVal val)
    {
        for (int i = 0; i < Keys.Count; ++i)
        {
            if (Keys[i].Equals(key))
            {
                Keys[i] = key;
                Vals[i] = val;
                return i;
            }
        }

        Keys.Add(key);
        Vals.Add(val);
        return Keys.Count;
    }

    /// <summary>
    /// Try remove key. Very slow.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val"></param>
    /// <returns>Returns removed kv index, -1 if remove failed.</returns>
    public int TryRemove(TKey key)
    {
        for (int i = 0; i < Keys.Count; ++i)
        {
            if (Keys[i].Equals(key))
            {
                Keys.RemoveAt(i);
                Vals.RemoveAt(i);
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Try get value.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val"></param>
    /// <returns>Returns key index, -1 if not exist.</returns>
    public int TryGet(TKey key, out TVal val)
    {
        for (int i = 0; i < Keys.Count; ++i)
        {
            if (Keys[i].Equals(key))
            {
                val = Vals[i];
                return i;
            }
        }

        val = default;
        return -1;
    }

    /// <summary>
    /// Direct insert kv. Make sure you tried get with -1 result.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val"></param>
    /// <returns>Returns inserted kv index.</returns>
    public int DirectInsert(TKey key, TVal val)
    {
        Keys.Add(key);
        Vals.Add(val);
        return Keys.Count;
    }

    /// <summary>
    /// Direct get pair. Make sure you tried get with >0 result.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val"></param>
    /// <returns>Returns key index, -1 if out of range.</returns>
    public int DirectGet(int idx, out TKey key, out TVal val)
    {
        /*
        if (idx < 0 || idx >= Keys.Count)
        {
            key = default;
            val = default;
            return -1;
        }
        */
        key = Keys[idx];
        val = Vals[idx];
        return idx;
    }
}

