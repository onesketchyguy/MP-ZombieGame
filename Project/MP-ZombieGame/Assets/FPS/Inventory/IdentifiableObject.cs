using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "object", menuName = "FPS/Identifiable object")]
public class IdentifiableObject : ScriptableObject
{
    private static uint sID = 0;
    public uint id = 0;

    public IdentifiableObject() { id = sID; sID++; }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (id == 0)
        {
            id = sID;
            sID++;
        }
    }
#endif
}
