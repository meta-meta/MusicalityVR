using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AppData
{
    public Dictionary<string, SerializableTransform> Transforms = new Dictionary<string, SerializableTransform>();
}

[System.Serializable]
public struct SerializableTransform
{
    public SerializableVector3 localPosition;
    public SerializableVector3 localScale;
    public SerializableQuaternion localRotation;
    public SerializableVector3 localRotationEuler;

    public SerializableTransform(Transform transform)
    { // TODO: FIXME changing from localPosition, etc. because of spawnFromSpawner. Need to account for that somehow 
        localPosition = transform.position;
        localScale = transform.lossyScale;
        localRotation = transform.rotation;
        localRotationEuler = transform.rotation.eulerAngles;
    }
}