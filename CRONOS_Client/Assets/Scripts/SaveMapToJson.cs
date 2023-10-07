using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveMapToJson : MonoBehaviour
{
    [System.Serializable]
    public struct ObjectToSave
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }

    [System.Serializable]
    public struct ToJson
    {
        public List<ObjectToSave> Map;
    }

    public string fileName = "";
    public LayerMask layerToFind = 1<<3;
    [SerializeField]
    public ToJson toJson;

    private void Awake()
    {
        Destroy(this.gameObject);
    }

}
