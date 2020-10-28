using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveHandler : MonoBehaviour
{
    #region Variables
    [SerializeField, Range(1,200),
        Tooltip("How often you save data each second. " +
        "Higher cps gives more accurate data, but will require more data power to view")]
    int capturesPerSecond = 10;

    [SerializeField, Tooltip("Whether to capture data or not." +
        " When replaying data, this will automatically be false.")]
    private bool captureData = true;

    private List<SaveDataBaseClass> saveDatas = new List<SaveDataBaseClass>();
    private ReplayDataScriptable replayData = null;

    public string JsonPath { get; private set; }
    public string JsonPrefix { get; private set; }
    #endregion Variables

    #region GameLoop
    private void Awake()
    {
        JsonPath = Directory.CreateDirectory(Application.dataPath + "/DebugStats/" + SceneManager.GetActiveScene().name + "/").FullName;
        JsonPrefix = "_" + SceneManager.GetActiveScene().name + "_" + Random.Range(0, 10000000) + "_debugStats.json";

        //If replaying a saved data, turn off capture
        replayData = (ReplayDataScriptable)Resources.Load("ReplayData", typeof(ReplayDataScriptable));
        if (replayData.RunReplay == true)
        {
            captureData = false;
        }

        //If capturing is on, gather data for the json file
        if (captureData == true)
        {
            StartCoroutine(AddDataTimer());
        }
    }

    private IEnumerator AddDataTimer()
    {
        WaitForSeconds wait = new WaitForSeconds(1.0f / capturesPerSecond);
        while (captureData == true)
        {
            OnAddData();
            yield return wait;
        }
    }
    #endregion GameLoop

    private void Update()
    {
        if (captureData == true)
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                OnSave();
            }
        }
    }

    #region Save
    private void OnAddData()
    {
        foreach (var save in saveDatas)
        {
            save.OnAddData();
        }
    }

    public void OnSave()
    {
        if (captureData == true)
        {
            saveDatas[0].data.capturesPerSecond = capturesPerSecond;

            if (saveDatas.Count > 0)
            {
                foreach (var save in saveDatas)
                {
                    save.OnSave();
                }
                Debug.Log("Saved Json!");
                captureData = false;
            }
            else
            {
                Debug.LogWarning("You need to have an object with a SaveDataBaseClass child!");
            }
        }
    }

    public void AddSaveData(SaveDataBaseClass saveData)
    {
        saveDatas.Add(saveData);
    }
    #endregion Save
}
