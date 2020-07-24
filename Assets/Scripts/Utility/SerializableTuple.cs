using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class SerializableTuple<T1, T2> {
    [SerializeField]
    private T1 key;

    [SerializeField]
    private T2 value;

    public T1 Key => key;
    public T2 Value => value;

    public SerializableTuple(T1 key, T2 value) {
        this.key = key;
        this.value = value;
    }
}

[Serializable]
public class StringFloatTuple : SerializableTuple<string, float> {
    public StringFloatTuple(string key, float item2) : base(key, item2) { }
}

