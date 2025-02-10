using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class ListHelper 
{
    private readonly static System.Random s_random;
    static ListHelper()
    {
        s_random = new System.Random();
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = s_random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
