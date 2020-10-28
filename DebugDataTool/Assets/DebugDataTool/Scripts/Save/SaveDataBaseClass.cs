using System.IO;
using UnityEngine;

public abstract class SaveDataBaseClass : MonoBehaviour
{
    [SerializeField, Tooltip("If the data is going to be uploaded to a server." +
                             " Keep in mind that you need to set the server info in the FTPUploader class!")] 
    private bool uploadToFTPServer = false;

    [Tooltip("Leave this empty.")]
    public DataHolder data = null;

    private SaveHandler saveHandler = null;
    private FTPUploader uploader = null;

    private void Awake()
    {
        if (uploadToFTPServer == true)
        {
            uploader = new FTPUploader();
        }
    }

    protected virtual void Start()
    {
        SaveDataBaseClass[] saveDataComponents = gameObject.GetComponents<SaveDataBaseClass>();
        foreach (var dataComponent in saveDataComponents)
        {
            if (dataComponent.data != null)
            {
                data = dataComponent.data;
                break;
            }
        }
        if (data == null)
        {
            data = new DataHolder();
        }

        if (saveHandler == null)
        {
            saveHandler = FindObjectOfType<SaveHandler>();
            if (saveHandler == null)
            {
                Debug.LogError("You need a SaveHandler in the world!");
            }
        }
        saveHandler.AddSaveData(this);
    }

    public virtual void OnSave() 
    {
        string jsonFileName = gameObject.name + saveHandler.JsonPrefix;
        string jsonFile = JsonUtility.ToJson(data);
        File.WriteAllText(saveHandler.JsonPath + jsonFileName, jsonFile);
        if (uploadToFTPServer == true)
        {
            uploader.UploadFile(saveHandler.JsonPath + jsonFileName);
        }
    }

    public virtual void OnLoad() {}

    public abstract void OnAddData();
}
