using UnityEngine;
using UnityEditor;

public class TopDownMiniMapTool : EditorWindow
{
    #region Variables
    //Editor variables
    private float miniMapScale = 1.0f;
    private float minimapPanSpeed = 0.5f;
    private Vector2 minimapHeightRange = new Vector2(5.0f, 200.0f);

    //Rects for the mini map window
    private Rect miniMapRect = new Rect(500, 160, 230, 250);
    private Rect textureViewRect = new Rect(0, 20, 230, 230);
    private Rect checkMouseBodyRect;
    private Rect checkMouseRibbonRect;

    //Objects
    private RenderTexture miniMapTexture;
    private GameObject miniMapCameraObject;
    private Camera miniMapCamera;

    //Checks for when holding buttons
    private bool holdingRibbon = false;
    private bool holdingMiniMap = false;
    private bool movingMiniMap = false;
    private bool holdingCtrl = false;

    //Start positions of objects
    private Vector2 mouseStartPos = Vector2.zero;
    private Vector3 miniMapCameraStartPos = Vector3.zero;
    private Vector2 miniMapRectStartPos = Vector2.zero;
    private Vector2 checkMouseBodyRectStartPos = Vector2.zero;
    private Vector2 checkMouseRibbonRectStartPos = Vector2.zero;
    private Vector2 miniMapRectStartSize = Vector2.zero;
    private Vector2 textureViewRectStartSize = Vector2.zero;
    private Vector2 checkMouseBodyRectStartSize = Vector2.zero;
    private Vector2 checkMouseRibbonRectStartSize = Vector2.zero;

    //For checking if Mouse is in screen
    private Vector2 flippedMousePosition = Vector2.zero;
    private Vector2 ribbonSize = Vector2.zero;
    private Vector2 sceneViewSize = Vector2.zero;
    private Vector2 sceneViewCorrectSize = Vector2.zero;
    private GUIStyle ribbonStyle = null;

    //Name of the GUI top ribbon
    private const string uiRibbonName = "GV Gizmo DropDown";

    //Misc variables
    private float oldScale = 0.0f;
    #endregion Variables

    #region Construction
    [MenuItem("Tools/TopDownMiniMap")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        TopDownMiniMapTool window = (TopDownMiniMapTool)EditorWindow.GetWindow(typeof(TopDownMiniMapTool));
        window.Show();
    }
    private void Awake()
    {
        miniMapCameraObject = new GameObject("MiniMapCamera");
        miniMapCameraObject.transform.Translate(0.0f, 100.0f, 0.0f);
        miniMapCameraObject.transform.Rotate(90.0f, 0.0f, 0.0f);
        miniMapCamera = miniMapCameraObject.AddComponent<Camera>();
        miniMapTexture = new RenderTexture(256, 256, 2);
        miniMapCamera.orthographic = true;
        miniMapCamera.orthographicSize = 20.0f;
        miniMapCamera.targetTexture = miniMapTexture;

        //Since the rect is created with the GUI top ribbon in mind,
        //we need to subtract the height of the ribbon to get correct values
        //When checking if the mouse is inside the mini map
        //Subtract the ribbon from the calculations
        checkMouseBodyRect = new Rect(
            miniMapRect.x,
            miniMapRect.y - GetGUIRibbonSize().y + (miniMapRect.height - miniMapRect.width),
            miniMapRect.width,
            miniMapRect.height - (miniMapRect.height - miniMapRect.width));

        //Make a Rect for the ribbon to be able to manipulate the window
        checkMouseRibbonRect = new Rect(
            miniMapRect.x,
            miniMapRect.y - GetGUIRibbonSize().y,
            miniMapRect.width,
            miniMapRect.height - checkMouseBodyRect.height);

        //Set start Sizes of the Rects for use when scaling
        miniMapRectStartSize = miniMapRect.size;
        textureViewRectStartSize = textureViewRect.size;
        checkMouseBodyRectStartSize = checkMouseBodyRect.size;
        checkMouseRibbonRectStartSize = checkMouseRibbonRect.size;

        //Sets the Old scale value for use when scaling the window
        oldScale = miniMapScale;
    }

    private void OnDestroy()
    {
        DestroyImmediate(miniMapCameraObject);
        DestroyImmediate(miniMapTexture);
    }
    #endregion Construction

    #region EventSubscription
    private void OnEnable()
    {
        SceneView.duringSceneGui += SceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= SceneGUI;
    }
    #endregion EventSubscription

    #region GUI
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Top down Mini Map Tool", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Click a position on the mini map to go there.");
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("To zoom the map, use the scroll wheel.");
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Hold Left Ctrl and scroll to move the camera in.");
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("You can move the position of the map with Middle Mouse.");

        EditorGUILayout.Separator();
        minimapPanSpeed = EditorGUILayout.Slider("Camera Pan Speed: ", minimapPanSpeed, 0.1f, 2.0f);
        EditorGUILayout.Separator();
        minimapHeightRange = EditorGUILayout.Vector2Field("Camera Height Range: ", minimapHeightRange);
        EditorGUILayout.Separator();
        miniMapScale = EditorGUILayout.Slider("MiniMap Scale: ", miniMapScale, 0.5f, 5.0f);

        if (GUI.changed == true)
        {
            //Change the scale of the window when the scale value is changed
            if (oldScale != miniMapScale)
            {
                SetMiniMapSize();
                oldScale = miniMapScale;
            }
        }
    }

    private void SceneGUI(SceneView sceneView)
    {
        //Hides the  Manipulators so the user can't accidentally move the camera.
        if (Selection.activeGameObject == miniMapCameraObject)
        {
            Tools.current = Tool.None;
        }

        //Creates MiniMap window and updates the texture
        miniMapRect = GUI.Window(0, miniMapRect, DoMyWindow, "MiniMap");

        //Turns on hold left Ctrl
        if (Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.LeftControl)
            {
                if (Event.current.type == EventType.KeyDown) Event.current.Use();

                holdingCtrl = true;
            }
        }

        //turns on hold MiniMap if pressed LMB on the mini map
        if (checkMouseBodyRect.Contains(Event.current.mousePosition))
        {
            //This makes it possible for MosueUp events to register
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0)
                {
                    holdingMiniMap = true;
                }
                else if (Event.current.button == 2)
                {
                    Cursor.visible = false;
                    //Disable the panning of the viewport camera when panning the mini map 
                    if (Event.current.type == EventType.MouseDown) Event.current.Use();

                    mouseStartPos = Event.current.mousePosition;
                    miniMapCameraStartPos = miniMapCameraObject.transform.position;
                    movingMiniMap = true;
                }
            }
            else if (Event.current.type == EventType.ScrollWheel)
            {
                //Disable the zooming of the viewport camera when zooming the mini map
                if (Event.current.type == EventType.ScrollWheel) Event.current.Use();

                if (holdingCtrl == false)
                {
                    miniMapCamera.orthographicSize += Event.current.delta.y;
                    miniMapCamera.orthographicSize = Mathf.Clamp(miniMapCamera.orthographicSize,
                                                                 minimapHeightRange.x, minimapHeightRange.y);
                }
                else
                {
                    miniMapCameraObject.transform.position += new Vector3(0.0f, Event.current.delta.y, 0.0f);

                    miniMapCameraObject.transform.position = new Vector3(miniMapCameraObject.transform.position.x,
                                                                         Mathf.Clamp(miniMapCameraObject.transform.position.y,
                                                                                     minimapHeightRange.x, minimapHeightRange.y),
                                                                         miniMapCameraObject.transform.position.z);
                }
            }
        }

        //turns on hold Ribbon if pressed LMB on the mini map ribbon
        if (checkMouseRibbonRect.Contains(Event.current.mousePosition))
        {
            //This makes it possible for MouseUp events to register
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0)
                {
                    SetStartPositions();
                    holdingRibbon = true;
                }
            }
        }

        if (holdingMiniMap == true)
        {
            sceneView.pivot = GetLocationOnMap();
        }

        if (holdingRibbon == true)
        {
            MoveMiniMapWindow();
        }

        if (movingMiniMap == true)
        {
            MoveMiniMapCamera();
        }

        //Let go of mouse buttons
        if (Event.current.type == EventType.MouseUp)
        {
            if (Event.current.button == 0 || Event.current.button == 2)
            {
                holdingMiniMap = false;
                holdingRibbon = false;
                movingMiniMap = false;
                Cursor.visible = true;
            }
        }

        //Let go of left Ctrl
        if (Event.current.type == EventType.KeyUp)
        {
            if (Event.current.keyCode == KeyCode.LeftControl)
            {
                holdingCtrl = false;
            }
        }

        //If the mouse goes out of screen, the holding of LMB will be released
        if ((holdingMiniMap == true || holdingRibbon == true ||
             movingMiniMap == true || holdingCtrl == true) &&
             MouseInScreen(sceneView) == false)
        {
            holdingMiniMap = false;
            holdingRibbon = false;
            movingMiniMap = false;
            holdingCtrl = false;
            Cursor.visible = true;
        }

        //Repaint to update the MiniMap window position
        HandleUtility.Repaint();
    }

    private void DoMyWindow(int windowID)
    {
        if (miniMapTexture != null && miniMapCameraObject != null)
        {
            miniMapCamera.targetTexture = miniMapTexture;
            GUI.DrawTexture(textureViewRect, miniMapTexture);
        }
    }
    #endregion GUI

    #region Positioning
    private void SetMiniMapSize()
    {
        miniMapRect.Set(miniMapRect.x, miniMapRect.y,
                        miniMapRectStartSize.x * miniMapScale,
                        miniMapRectStartSize.y * miniMapScale);

        textureViewRect.Set(textureViewRect.x, textureViewRect.y,
                            textureViewRectStartSize.x * miniMapScale,
                            textureViewRectStartSize.y * miniMapScale);

        checkMouseBodyRect.Set(checkMouseBodyRect.x, checkMouseBodyRect.y,
                               checkMouseBodyRectStartSize.x * miniMapScale,
                               checkMouseBodyRectStartSize.y * miniMapScale);

        checkMouseRibbonRect.Set(checkMouseRibbonRect.x, checkMouseRibbonRect.y,
                                 checkMouseRibbonRectStartSize.x * miniMapScale,
                                 checkMouseRibbonRectStartSize.y * miniMapScale);
    }
    private void SetStartPositions()
    {
        checkMouseBodyRectStartPos = checkMouseBodyRect.position;
        checkMouseRibbonRectStartPos = checkMouseRibbonRect.position;
        miniMapRectStartPos = miniMapRect.position;
        mouseStartPos = Event.current.mousePosition;
    }

    private void MoveMiniMapWindow()
    {
        //Creates an offset for the mouse and uses that to move the different Rects
        Vector2 mouseOffsetPosition = Event.current.mousePosition - mouseStartPos;

        Vector2 checkMouseBodyRectCurrentPos = checkMouseBodyRectStartPos + mouseOffsetPosition;
        checkMouseBodyRect.Set(checkMouseBodyRectCurrentPos.x,
                               checkMouseBodyRectCurrentPos.y,
                               checkMouseBodyRect.size.x,
                               checkMouseBodyRect.size.y);

        Vector2 checkMouseRibbonRectCurrentPos = checkMouseRibbonRectStartPos + mouseOffsetPosition;
        checkMouseRibbonRect.Set(checkMouseRibbonRectCurrentPos.x,
                                 checkMouseRibbonRectCurrentPos.y,
                                 checkMouseRibbonRect.size.x,
                                 checkMouseRibbonRect.size.y);

        Vector2 miniMapRectCurrentPos = miniMapRectStartPos + mouseOffsetPosition;
        miniMapRect.Set(miniMapRectCurrentPos.x,
                        miniMapRectCurrentPos.y,
                        miniMapRect.size.x,
                        miniMapRect.size.y);
    }

    private void MoveMiniMapCamera()
    {
        Vector2 mouseOffsetPosition = Event.current.mousePosition - mouseStartPos;
        Vector3 mouseOffsetPositionConverted = new Vector3(mouseOffsetPosition.x * minimapPanSpeed,
                                                           0.0f, -mouseOffsetPosition.y * minimapPanSpeed);
        Vector3 miniMapCameraOffset = miniMapCameraStartPos - mouseOffsetPositionConverted;
        miniMapCameraObject.transform.position = miniMapCameraOffset;
    }

    private Vector2 GetGUIRibbonSize()
    {
        ribbonStyle = uiRibbonName;
        ribbonSize = ribbonStyle.CalcSize(SceneView.lastActiveSceneView.titleContent);

        return ribbonSize;
    }

    private bool MouseInScreen(SceneView sceneView)
    {
        //Get the size of the ribbon on top of the viewport
        ribbonStyle = uiRibbonName;
        ribbonSize = ribbonStyle.CalcSize(sceneView.titleContent);
        //Subtract the ribbon size from the actual screen size
        sceneViewSize = sceneView.position.size;
        sceneViewCorrectSize = sceneViewSize;
        sceneViewCorrectSize.y = sceneViewSize.y - ribbonSize.y;
        //Flip the mouse position.y to get the correct direction of screen
        flippedMousePosition = Event.current.mousePosition;
        flippedMousePosition.y = sceneViewCorrectSize.y - flippedMousePosition.y;

        //If the mouse is inside the window, return true
        if (flippedMousePosition.y < sceneViewCorrectSize.y && flippedMousePosition.y > 0 &&
            flippedMousePosition.x < sceneViewCorrectSize.x && flippedMousePosition.x > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private Vector3 GetLocationOnMap()
    {
        Vector2 MousePosInMiniMap = Event.current.mousePosition - checkMouseBodyRect.position;

        //Get a percentage of the mouse in mini map window. subtracted 50% to be able to use as offset of camera position
        Vector2 screenPositionPercent = new Vector2((MousePosInMiniMap.x / checkMouseBodyRect.size.x) - 0.5f,
                                                    (MousePosInMiniMap.y / checkMouseBodyRect.size.y) - 0.5f);

        //Get a offset of the camera position, depending on where the mouse is in the mini map window
        Vector3 minimapCameraOffset = miniMapCameraObject.transform.position +
                                      new Vector3((miniMapCamera.orthographicSize * 2.0f) * screenPositionPercent.x,
                                      0.0f, (miniMapCamera.orthographicSize * 2.0f) * -screenPositionPercent.y);

        RaycastHit hit;
        if (Physics.Raycast(minimapCameraOffset, Vector3.down, out hit, Mathf.Infinity))
        {
            Vector3 invertedWorldPos = new Vector3(hit.point.x, hit.point.y, hit.point.z);
            return invertedWorldPos;
        }
        return Vector3.zero;
    }
    #endregion Positioning
}
