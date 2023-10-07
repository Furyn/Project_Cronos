using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SaveMapToJson))]
public class SaveMapToJsonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SaveMapToJson script = (SaveMapToJson)target;

        if (GUILayout.Button("Load object in map"))
        {
            Debug.Log("Load object in map");
            script.toJson.Map.Clear();
            var goArray = FindObjectsOfType<GameObject>();

            for (var i = 0; i < goArray.Length; i++)
            {
                if ( LayerMask.GetMask( LayerMask.LayerToName(goArray[i].layer) ) == script.layerToFind)
                {
                    SaveMapToJson.ObjectToSave objectToSave;
                    objectToSave.position = goArray[i].transform.position;
                    objectToSave.rotation = goArray[i].transform.rotation.eulerAngles;
                    objectToSave.scale = goArray[i].transform.localScale;
                    script.toJson.Map.Add(objectToSave);
                }
            }
            Debug.Log("Object loaded");
        }
        
        if (GUILayout.Button("Save Map"))
        {
            Debug.Log("Save map in proces");
            string json = JsonUtility.ToJson(script.toJson);
            System.IO.File.WriteAllText(Application.dataPath + "/" + script.fileName +".json", json);
            Debug.Log("Map saved !");
        }
        
    }
}
