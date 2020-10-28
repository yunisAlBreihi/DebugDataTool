using UnityEngine;

[CreateAssetMenu(fileName = "ReplayData", menuName = "ScriptableObjects/ReplayData", order = 1)]
public class ReplayDataScriptable : ScriptableObject
{
    private DataHolder data = null;
    private bool runReplay = false;

    public DataHolder Data => data;
    public bool RunReplay => runReplay;

    public void StartReplay(DataHolder data) 
    {
        this.data = data;
        runReplay = true;
    }

    public void Clear()
    {
        runReplay = false;
        data = null;
    }
}
