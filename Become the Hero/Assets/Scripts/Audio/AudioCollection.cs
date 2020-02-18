using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "NewAudioCollection", menuName = "System/Audio/Audio Collection", order = 0)]
public class AudioCollection : SerializedScriptableObject
{
    [SerializeField]
    private Dictionary<string, AudioClip> collection = new Dictionary<string, AudioClip>();


    public AudioClip GetClip(string s)
    {
        if (collection.ContainsKey(s))
            return collection[s];

        return null;
    }


    public AudioClip GetRandomClip()
    {
        return collection.Values.ElementAt(Random.Range(0, collection.Count));
    }
}
