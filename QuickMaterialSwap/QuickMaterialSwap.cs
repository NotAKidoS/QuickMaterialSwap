using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

class MaterialSwap : EditorWindow
{
    // The object whose materials are being edited
    private GameObject targetObject;

    private List<Material> materials;
    private bool showMaterials = true; //default mode is to show materials

    private Vector2 scrollPosition;

    // Add a menu item to create the window
    [MenuItem("NotAKid/Quick Material Swap")]
    public static void ShowWindow()
    {
        // Show the window
        EditorWindow.GetWindow(typeof(MaterialSwap), false, "Quick Material Swap");
    }

    private void OnGUI()
    {
        float width = EditorGUIUtility.currentViewWidth * 0.985f;
        float scaledWidth = width / 2.6f;

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Select the target object
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Selected Object", EditorStyles.boldLabel);
        targetObject = (GameObject)EditorGUILayout.ObjectField(targetObject, typeof(GameObject), true);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Use Selection"))
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects != null && selectedObjects.Length > 0)
            {
                targetObject = selectedObjects.Select(obj => obj.GetComponentInParent<Transform>()).FirstOrDefault().gameObject;
            }
            else
            {
                targetObject = null;
            }
        }

        GUILayout.Space(10);

        if (targetObject != null)
        {
            //material parsing
            showMaterials = GUILayout.Toggle(showMaterials, "Sort by Shared Materials");
            if(showMaterials)
            {
                //display the info headers
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Shared Materials:",EditorStyles.boldLabel, GUILayout.Width(scaledWidth));
                GUILayout.Space(scaledWidth / 2);
                EditorGUILayout.LabelField("GameObjects:",EditorStyles.boldLabel, GUILayout.Width(scaledWidth));
                EditorGUILayout.EndHorizontal();

                // Get all the distinct materials of the targetObject and its children
                materials = targetObject.GetComponentsInChildren<Renderer>()
                    .SelectMany(r => r.sharedMaterials)
                    .Distinct()
                    .ToList();


                // Display the materials in a list
                for (int i = 0; i < materials.Count; i++)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginHorizontal();

                    Material newMaterial = (Material)EditorGUILayout.ObjectField(materials[i], typeof(Material), false,GUILayout.Width(scaledWidth));
                    // display the gameobject using the material
                    EditorGUILayout.LabelField("Used By:",GUILayout.Width(scaledWidth/2));

                    EditorGUILayout.BeginVertical();
                    targetObject.GetComponentsInChildren<Renderer>()
                        .Where(r => r.sharedMaterials.Contains(materials[i]))
                        .Select(r => r.gameObject)
                        .ToList()
                        .ForEach(go => EditorGUILayout.ObjectField(go, typeof(GameObject), true,GUILayout.Width(scaledWidth)));
                    EditorGUILayout.EndVertical();

                    if (newMaterial != materials[i])
                    {
                        Debug.Log("Material changed! " + materials[i].name + " changed to " + newMaterial.name);
                        //change the material
                        ReplaceMaterial(materials[i], newMaterial);
                        materials[i] = newMaterial;
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(width));
                EditorGUILayout.HelpBox("Changing a material in the list will automatically change the material for all GameObjects that use it.", MessageType.Info);
            }
            else    
            {
                //display the info headers
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("GameObjects:",EditorStyles.boldLabel, GUILayout.Width(scaledWidth));
                GUILayout.Space(scaledWidth / 2);
                EditorGUILayout.LabelField("Materials:",EditorStyles.boldLabel, GUILayout.Width(scaledWidth));
                EditorGUILayout.EndHorizontal();

                // Get all the gameobjects of the targetObject and its children that have a renderer component
                List<GameObject> gameObjects = targetObject.GetComponentsInChildren<Transform>()
                    .Select(t => t.gameObject)
                    .Where(go => go.GetComponent<Renderer>() != null)
                    .ToList();

                // Display the gameobjects in a list
                for (int i = 0; i < gameObjects.Count; i++)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.ObjectField(gameObjects[i], typeof(GameObject), true,GUILayout.Width(scaledWidth));

                    // Display the materials used by the gameobject
                    EditorGUILayout.LabelField("Used Mats:",GUILayout.Width(scaledWidth/2));
                    EditorGUILayout.BeginVertical();
                    Renderer renderer = gameObjects[i].GetComponent<Renderer>();
                    Material[] newMaterials = new Material[renderer.sharedMaterials.Length];
                    for (int j = 0; j < renderer.sharedMaterials.Length; j++)
                    {
                        Material newMaterial = (Material)EditorGUILayout.ObjectField(renderer.sharedMaterials[j], typeof(Material), false,GUILayout.Width(scaledWidth));
                        newMaterials[j] = newMaterial;
                        if(newMaterial != renderer.sharedMaterials[j])
                        {
                            Undo.RecordObject(renderer, "Changed Material");
                            Debug.Log("Material changed on " + gameObjects[i].name + " from " + renderer.sharedMaterials[j].name + " to " + newMaterial.name);
                        }
                    }
                    renderer.sharedMaterials = newMaterials;
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(width));
                EditorGUILayout.HelpBox("Changing a material on a GameObject will not change the same material on other GameObjects.", MessageType.Info);
            }
            EditorGUILayout.HelpBox("Changes are properly recorded using Unity's Undo System and can be undone using CTRL+Z.", MessageType.Info);
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(width));
            EditorGUILayout.HelpBox("You need to select an object first!", MessageType.Info);
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

    }

    private void ReplaceMaterial(Material oldMaterial, Material newMaterial)
    {
        targetObject.GetComponentsInChildren<Renderer>()
            .ToList()
            .ForEach(r =>
            {
                Undo.RecordObject(r, "Swap Material");
                List<Material> sharedMaterials = r.sharedMaterials.ToList();
                for (int i = 0; i < sharedMaterials.Count; i++)
                {
                    if (sharedMaterials[i] == oldMaterial)
                    {
                        sharedMaterials[i] = newMaterial;
                    }
                }
                r.sharedMaterials = sharedMaterials.ToArray();
            });
        Undo.FlushUndoRecordObjects();
    }
}