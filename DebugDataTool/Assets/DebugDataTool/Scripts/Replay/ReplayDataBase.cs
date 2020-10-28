using UnityEngine;

public abstract class ReplayDataBase : MonoBehaviour
{
    #region Variables
    protected ReplayDataScriptable replayData = null;
    protected DataHolder data = null;

    protected float lerpDelta = 0.0f;
    protected int replayIndex = 0;
    #endregion Variables

    #region GameLoop
    protected virtual void Awake()
    {
        replayData = (ReplayDataScriptable)Resources.Load("ReplayData", typeof(ReplayDataScriptable));
    }

    protected virtual void Start()
    {
        if (replayData.RunReplay == true)
        {
            data = replayData.Data;
        }
    }

    private void OnDisable()
    {
        replayData.Clear();
    }

    private void Update()
    {
        if (replayData.RunReplay == true)
        {
            OnReplayBegin();
            if (lerpDelta < 1.0f)
            {
                if (replayIndex + 1 < data.positions.Count)
                {
                    OnReplayPogressed();
                }
                else
                {
                    replayData.Clear();
                }
                lerpDelta += Time.deltaTime * data.capturesPerSecond;
            }
            else
            {
                replayIndex++;
                lerpDelta = 0.0f;
            }
            if (replayIndex == data.positions.Count - 1)
            {
                OnReplayComplete();
            }
        }
    }
    #endregion GameLoop

    #region AbstractMethods
    protected abstract void OnReplayPogressed();
    protected abstract void OnReplayBegin();
    protected abstract void OnReplayComplete();
    #endregion AbstractMethods
}
