using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using System.IO;

#if(UNITY_EDITOR)
public class DebugHolder
{
    public string dataName = "";
    public int popupIndex = int.MaxValue;
    public string jsonFile = "";
    public DebugLine debugLine = null;
    public DataHolder dataHolder = null;
}

public class DebugDataTool : EditorWindow
{
    #region Variables
    //Debug data containers
    private List<DebugHolder> debugHolders = new List<DebugHolder>();
    private DebugHolder currentDebugHolder = null;

    //This is separated from the DebugHolder list since it is needed for the dropdown menu
    private List<string> dataNames = new List<string>();

    //Prefab objects gotten from the resources folder
    private GameObject debugLinePrefab = null;
    private GameObject cameraPrefab = null;
    private ReplayDataScriptable replayData = null;

    //Debug camera that moves on the different debugLines
    private GameObject debugCamera = null;
    private Vector3 cameraOffset = Vector3.zero;

    //Used for the data time line and also for the camera positions
    private int positionIndex = 0;
    private int lastPositionIndex = 0;

    //The index for each data in the dropdown menu
    private int dropDownIndex = 0;
    private int lastDropDownIndex = 0;

    //Rects for the different UI elements
    private Rect folderLabelRect = new Rect(10, 5, 300, 20);
    private Rect buttonRect = new Rect(10, 30, 100, 20);
    private Rect dropDownLabelRect = new Rect(10, 70, 300, 20);
    private Rect dropDownMenuRect = new Rect(10, 95, 100, 10);
    private Rect sliderLabelRect = new Rect(10, 135, 300, 20);
    private Rect sliderRect = new Rect(10, 160, 300, 20);
    private Rect replayLabelRect = new Rect(10, 200, 300, 20);
    private Rect replayButtonRect = new Rect(10, 225, 100, 20);

    //Reference to the window itself
    private static DebugDataTool window = null;
    #endregion Variables

    #region Init
    [MenuItem("Tools/DebugDataTool")]
    private static void Init()
    {   
        window = (DebugDataTool)EditorWindow.GetWindow(typeof(DebugDataTool));
        window.Show();
    }

    private void OnEnable()
    {
        //Get the assets needed from the resource folder
        debugLinePrefab = (GameObject)Resources.Load("DebugLine", typeof(GameObject));
        cameraPrefab = (GameObject)Resources.Load("DebugLineCamera", typeof(GameObject));
        replayData = (ReplayDataScriptable)Resources.Load("ReplayData", typeof(ReplayDataScriptable));
    }

    private void OnDisable()
    {
        DestroyDebugObjects();
    }
    #endregion Init

    #region GUIEvents
    private void OnGUI()
    {
        //UI for selecting data folder
        EditorGUI.LabelField(folderLabelRect, "Select a folder where you have debug data jsons.");
        if (GUI.Button(buttonRect, "Load Path"))
        {
            if (debugHolders.Count > 0)
            {
                DestroyDebugObjects();
                debugHolders.Clear();
            }

            GetDebugFiles();
            CreateDebugLines();
            CreateCamera();
        }

        //Dropdown menu and button
        if (debugHolders.Count != 0)
        {
            EditorGUI.LabelField(dropDownLabelRect, "Choose data to look at.");
            dropDownIndex = EditorGUI.Popup(dropDownMenuRect, dropDownIndex, dataNames.ToArray());

            EditorGUI.LabelField(replayLabelRect, "Press button to replay current selected data.");
            if (GUI.Button(replayButtonRect, "Replay Data"))
            {
                if (replayData != null)
                {
                    replayData.Clear();
                    StartReplay();
                }
            }
        }

        //Timeline slider
        if (currentDebugHolder != null)
        {
            EditorGUI.LabelField(sliderLabelRect, "Timeline. Expand window if not showing correctly.");
            positionIndex = EditorGUI.IntSlider(sliderRect, positionIndex, 0, currentDebugHolder.dataHolder.positions.Count - 1);
            if (positionIndex >= currentDebugHolder.dataHolder.positions.Count - 1)
            {
                positionIndex = currentDebugHolder.dataHolder.positions.Count - 1;
            }
        }

        //Move camera and select it when changing things in the GUI
        if (GUI.changed)
        {
            if (debugHolders.Count > 0)
            {
                if (debugCamera != null)
                {
                    if (dropDownIndex != lastDropDownIndex)
                    {
                        SelectData(dropDownIndex);
                        lastDropDownIndex = dropDownIndex;
                    }

                    if (positionIndex != lastPositionIndex)
                    {
                        lastPositionIndex = positionIndex;
                    }
                    MoveCamera(positionIndex);
                    Selection.activeObject = debugCamera;
                }
            }
        }
    }
    #endregion GUIEvents

    #region Destroy
    private void DestroyDebugObjects()
    {
        //Destroy the used objects when closing the window
        DestroyImmediate(debugCamera);
        foreach (var debugHolder in debugHolders)
        {
            DestroyImmediate(debugHolder.debugLine.gameObject);
        }
        Resources.UnloadUnusedAssets();
    }
    #endregion Destroy

    #region Replay
    private void StartReplay()
    {
        replayData.StartReplay(currentDebugHolder.dataHolder);
        window.Close();
        EditorCoroutineUtility.StartCoroutine(StartPlayMode(), this);
    }

    IEnumerator StartPlayMode()
    {
        yield return new WaitForSeconds(0.2f);
        EditorApplication.isPlaying = true;
    }
    #endregion Replay

    #region Camera
    private void CreateCamera()
    {
        debugCamera = Instantiate(cameraPrefab);

        //Set an offset, that will be added to the positioning when moving.
        cameraOffset = debugCamera.transform.position;
        MoveCamera(0);
        Selection.activeObject = debugCamera;
    }

    private void MoveCamera(int index)
    {
        debugCamera.transform.position = currentDebugHolder.dataHolder.positions[index] + cameraOffset;
        debugCamera.transform.forward = currentDebugHolder.dataHolder.lookDirections[index];
    }
    #endregion Camera

    #region GetData
    private void LoadJsonFile(DebugHolder debugHolder)
    {
        string currentFile = File.ReadAllText(debugHolder.jsonFile);
        DataHolder currentData = JsonUtility.FromJson<DataHolder>(currentFile);
        debugHolder.dataHolder = currentData;
    }

    private void GetDebugFiles()
    {
        string jsonPath = EditorUtility.OpenFolderPanel("Choose DebugDeta Path", Application.dataPath, "");

        //Only add the files that end with .json
        string[] dataFiles = Directory.GetFiles(jsonPath);
        int nameIndex = 0;
        for (int i = 0; i < dataFiles.Length; i++)
        {
            if (dataFiles[i].EndsWith(".json"))
            {
                currentDebugHolder = new DebugHolder();
                currentDebugHolder.jsonFile = dataFiles[i];
                string dataName = "Data" + nameIndex;
                currentDebugHolder.popupIndex = nameIndex;
                currentDebugHolder.dataName = dataName;
                dataNames.Add(dataName);
                debugHolders.Add(currentDebugHolder);
                nameIndex++;
            }
        }
        currentDebugHolder = debugHolders[0];
    }

    private void SelectData(int index)
    {
        currentDebugHolder = debugHolders[index];
    }
    #endregion GetData

    #region DebugLines
    private void CreateDebugLines()
    {
        foreach (var debugHolder in debugHolders)
        {
            LoadJsonFile(debugHolder);
            CreateSingleLine(debugHolder);
        }
    }

    private void CreateSingleLine(DebugHolder debugHolder)
    {
        GameObject gameObject = Instantiate(debugLinePrefab);
        DebugLine line = gameObject.GetComponent<DebugLine>();
        if (line != null)
        {
            line.Create(debugHolder.dataHolder);
            debugHolder.debugLine = line;
        }
    }
    #endregion DebugLines
}
#endif