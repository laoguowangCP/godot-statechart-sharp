using System;
using System.Collections.Generic;
using System.Linq;

namespace LGWCP.Util;


public class ArrayHelper
{
    public static void KVListInsert<TKey, TVal>(
        TKey key,
        TVal val,
        List<TKey> keys,
        List<List<TVal>> vals)
        where TKey : IEquatable<TKey>
    {
        bool isKeyExist = false;
        for (int i = 0; i < keys.Count; ++i)
        {
            var k = keys[i];
            if (k.Equals(key))
            {
                isKeyExist = true;
                var ls = vals[i];
                ls.Add(val);
                break;
            }
        }

        if (!isKeyExist)
        {
            // new key and val list
            keys.Add(key);
            var ls = new List<TVal>() { val };
            vals.Add(ls);
        }
    }

    public static void KVListToArray<TKey, TVal>(
        List<TKey> keys,
        List<List<TVal>> vals,
        out TKey[] keysArray,
        out TVal[][] valsArray)
        where TKey : IEquatable<TKey>
    {
        keysArray = keys.ToArray();
        valsArray = vals.Select(ls => ls.ToArray()).ToArray();
    }

    /// <summary>
    /// Initialization of certain small frozen dictionary, where value is array of TVal
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TVal"></typeparam>
    /// <param name="valCntPerKey"></param>
    /// <param name="keys"></param>
    /// <param name="vals"></param>
    public static void KVArrayDictAlloc<TKey, TVal>(
        Span<int> valCntPerKey,
        out TKey[] keys,
        out TVal[][] vals
    )
    {
        int keyCnt = valCntPerKey.Length;
        keys = new TKey[keyCnt];
        vals = new TVal[keyCnt][];
        for (int i = 0; i < keyCnt; ++i)
        {
            vals[i] = new TVal[valCntPerKey[i]];
        }
    }

    public static int ArrayDictTryGet<TKey, TVal>(
        TKey key,
        TKey[] keys,
        TVal[] vals,
        ref TVal val)
        where TKey : IEquatable<TKey>
    {
        for (int i = 0; i < keys.Length; ++i)
        {
            if (key.Equals(keys[i]))
            {
                val = vals[i];
                return i;
            }
        }

        return -1;
    }

}
