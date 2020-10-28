using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices;

public class MoveCameraWithObject : EditorWindow
{
    #region Variables
    //Editor variables
    private KeyCode keyToActivate = KeyCode.C;
    private Color labelColor = Color.red;
    private Color sliderColor = Color.black;

    //OnScreen variables
    private float cameraDampingSlider = 1.0f;

    //For when starting to hold LMB
    private Vector3 objectStartPosition = Vector3.zero;
    private Vector3 objectCurrentPosition = Vector3.zero;
    private Vector3 CameraStartPosition = Vector3.zero;
    private Vector3 objectLocalOffset = Vector3.zero;
    private Vector3 cameraOffsetPosition = Vector3.zero;
    private Vector3 handlePosition = Vector3.zero;
    private bool activate = true;

    Vector3 currentHandlePos = Vector3.zero;

    //For mouse events
    private Transform currentTransform = null;
    private Vector2 flippedMousePosition = Vector2.zero;
    private Vector2 ribbonSize = Vector2.zero;
    private Vector2 sceneViewSize = Vector2.zero;
    private Vector2 sceneViewCorrectSize = Vector2.zero;
    private GUIStyle ribbonStyle = null;
    private bool LMBHeld = false;

    //OnScreen GUI
    private GUIStyle labelStyle = GUIStyle.none;
    private Rect labelRect = new Rect(25, 25, 100, 30);

    private Rect dampingSliderRect = new Rect(25, 60, 200, 30);
    private Rect dampingLabelRect = new Rect(25, 60, 300, 30);
    private Rect activateLabelRect = new Rect(25, 100, 300, 30);
    #endregion Variables

    //[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //public static extern int SetCursorPos(int x, int y);
    //private float mouseX;
    //private float mouseY;

    #region CreateGUI
    [MenuItem("Tools/MoveCameraWithObject")]
    public static void ShowWindow()
    {
        GetWindow(typeof(MoveCameraWithObject));
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Select object/s to move the camera with.", EditorStyles.boldLabel);
        keyToActivate = (UnityEngine.KeyCode)EditorGUILayout.EnumPopup("Key to activate: ", keyToActivate);
        labelColor = EditorGUILayout.ColorField("Label Color: ", labelColor);
        sliderColor = EditorGUILayout.ColorField("Slider Color: ", sliderColor);
    }
    #endregion CreateGUI

    #region EventSubscription
    void OnEnable()
    {
        SceneView.duringSceneGui += SceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= SceneGUI;
    }
    #endregion EventSubscription

    #region Update

    private void SceneGUI(SceneView sceneView)
    {
        //if (Selection.activeTransform != null)
        //{
        //    Tools.current = Tool.None;
        //    handlePosition = Handles.PositionHandle(currentHandlePos, Selection.activeTransform.rotation);
        //    Selection.activeTransform.position = handlePosition;
        //}

        //Activate camera follow with keyboard button
        if (Event.current.type == EventType.KeyUp)
        {
            if (Event.current.keyCode == keyToActivate)
            {
                activate = !activate;
            }
        }

        //Draw the viewport GUI
        DrawViewportGUI();

        if (activate == true)
        {
            //if (handlePosition != currentHandlePos)
            //{
            //    Debug.Log("Najjj!");
            //    if (LMBHeld == false)
            //    {
            //        LMBHeld = true;
            //    }
            //}
            //else
            //{
            //    if (LMBHeld == true)
            //    {
            //        LMBHeld = false;
            //    }
            //}

            if (Event.current.type == EventType.MouseDown)
            {
                //Gets the start position of the object, to get the offset later on
                if (LMBHeld == false)
                {
                    SetStartPositions(sceneView);
                    LMBHeld = true;
                }
            }
            if (Event.current.type == EventType.MouseUp || currentTransform != Selection.activeTransform)
            {
                objectStartPosition = Vector3.zero;
                CameraStartPosition = Vector3.zero;
                Cursor.visible = true;

                LMBHeld = false;
            }
            MoveCamera(sceneView);
            //Selection.activeTransform.position += Vector3.right * 0.5f;
            //sceneView.pivot = Selection.activeTransform.position;

        }
    }

    //private void Update()
    //{
    //    Debug.Log(LMBHeld);
    //}

    //private void Update()
    //{
    //    if (activate == true)
    //    {
    //        if (SceneView.lastActiveSceneView != null)
    //        {
    //            if (Selection.activeTransform != null)
    //            {
    //                Selection.activeTransform.position += Vector3.right;
    //                SceneView.lastActiveSceneView.pivot = Selection.activeTransform.position;
    //            }

    //        }
    //    }
    //}

    private void SetStartPositions(SceneView sceneView)
    {
        if (Selection.activeTransform != null)
        {
            currentTransform = Selection.activeTransform;
            objectStartPosition = currentTransform.position;
        }

        if (sceneView != null)
        {
            CameraStartPosition = sceneView.pivot;
        }
    }

    private void MoveCamera(SceneView sceneView)
    {
        if (LMBHeld == true)
        {
            //Debug.Log(Selection.activeTransform.position);
            if (currentTransform != null)
            {
                if (objectCurrentPosition != currentTransform.position)
                {
                    //SetCursorPos((int)mouseX, (int)mouseY);
                    //SetCursorPos((int)Event.current.mousePosition.x, (int)Event.current.mousePosition.y);

                    //Turn off cursor only when you actually can move the object
                    //Cursor.visible = false;
                    if (MouseInScreen(sceneView) == false)
                    {
                        Cursor.visible = true;
                    }

                    float posXFromMiddle = flippedMousePosition.x - MiddleOfScreen().x;
                    float posXFromMiddlePercent = posXFromMiddle / 1000.0f;
                    float positivePosXFromMiddlePercent = Mathf.Abs(posXFromMiddlePercent);

                    float posYFromMiddle = flippedMousePosition.y - MiddleOfScreen().y;
                    float posYFromMiddlePercent = posYFromMiddle / 1000.0f;
                    float positivePosYFromMiddlePercent = Mathf.Abs(posYFromMiddlePercent);

                    //SetCursorPos(960, 540);

                    //Vector2 MousePosFromScreen =new Vector2(positivePosXFromMiddlePercent, positivePosYFromMiddlePercent);

                    //Transform a global position into local,
                    //so it can be used to move the camera relative to the object
                    objectCurrentPosition = Selection.activeTransform.position;
                    objectLocalOffset = objectCurrentPosition - objectStartPosition;

                    //Move the camera relative to the object
                    cameraOffsetPosition = (objectLocalOffset * cameraDampingSlider) + CameraStartPosition;
                    //totalCameraOffsetPosition.x = cameraOffsetPosition.x * positivePosXFromMiddlePercent;
                    //totalCameraOffsetPosition.y = cameraOffsetPosition.y * positivePo sYFromMiddlePercent;
                    sceneView.pivot = cameraOffsetPosition;
                    //Debug.Log(Camera.current);
                    //sceneView.camera.transform.position = cameraOffsetPosition;
                    //sceneView.pivot += Vector3.right;
                    sceneView.Repaint();

                    //sceneView.pivot = Selection.activeTransform.position;

                }
            }
        }
    }

    private bool MouseInScreen(SceneView sceneView)
    {
        //Get the size of the ribbon on top of the viewport
        ribbonStyle = "GV Gizmo DropDown";
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

    private Vector2 MiddleOfScreen()
    {
        return sceneViewCorrectSize * 0.5f;
    }

    private void DrawViewportGUI()
    {
        if (activate == true)
        {
            Handles.BeginGUI();
            //Camera Following Damping settings 
            labelStyle.fontSize = 20;
            labelStyle.padding.left = 0;
            labelStyle.normal.textColor = labelColor;
            GUI.Label(activateLabelRect, "Deactivate camera follow object with " + keyToActivate.ToString(), labelStyle);

            //Damping slider settings
            GUI.color = sliderColor;
            labelStyle.normal.textColor = sliderColor;
            labelStyle.fontSize = 15;
            GUI.Label(labelRect, "Camera Follow Damping", labelStyle);
            labelStyle.padding.left = 220;
            cameraDampingSlider = GUI.HorizontalSlider(dampingSliderRect, cameraDampingSlider, 0.0f, 1.0f);
            GUI.Label(dampingLabelRect, cameraDampingSlider.ToString("F2"), labelStyle);
            Handles.EndGUI();
        }
        else
        {
            //Deactivate Text settings
            Handles.BeginGUI();
            labelStyle.normal.textColor = labelColor;
            labelStyle.padding.left = 0;
            labelStyle.fontSize = 20;
            GUI.Label(labelRect, "Camera is not following object!", labelStyle);
            GUI.Label(activateLabelRect, "Activate camera follow object with " + keyToActivate.ToString(), labelStyle);
            Handles.EndGUI();
        }
    }
    #endregion Update
}
