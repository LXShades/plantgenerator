using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CustomEditor(typeof(PlantGenerator))]
public class PlantGeneratorEditor : Editor {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    override public void OnInspectorGUI()
    {
        PlantGenerator generator = target as PlantGenerator;

        base.OnInspectorGUI();
    }

    // Renders any editable points in the plant generator
    void OnSceneGUI()
    {
        PlantGenerator generator = target as PlantGenerator;

        for (int i = 0; i < generator.editablePoints.Length; i++)
        {
            // Render each piont with a purple handle
            Handles.color = new Color(1.0f, 0.0f, 1.0f, 0.1f);
            generator.editablePoints[i] = generator.transform.InverseTransformPoint(Handles.DoPositionHandle(generator.transform.TransformPoint(generator.editablePoints[i]), Quaternion.identity));
        }

    }
}
