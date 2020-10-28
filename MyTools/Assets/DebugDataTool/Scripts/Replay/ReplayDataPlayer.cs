using UnityEngine;

public class ReplayDataPlayer : ReplayDataBase
{
    #region Variables
    private PlayerCharacterController controller = null;
    #endregion Variables

    #region GameLoop
    protected override void Awake()
    {
        base.Awake();
        controller = gameObject.GetComponent<PlayerCharacterController>();
    }
    #endregion GameLoop

    #region Movement
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    private void LerpMovement(Vector3 pos1, Vector3 pos2, Vector3 dir1, Vector3 dir2, float delta)
    {
        transform.position = Vector3.Lerp(pos1, pos2, delta);
        transform.forward = Vector3.Lerp(dir1, dir2, delta);
    }
    #endregion Movement

    #region AbstractMethods
    protected override void OnReplayBegin()
    {
        controller.enabled = false;
    }

    protected override void OnReplayPogressed()
    {
        LerpMovement(data.positions[replayIndex], data.positions[replayIndex + 1],
        data.lookDirections[replayIndex], data.lookDirections[replayIndex + 1], lerpDelta);
    }

    protected override void OnReplayComplete()
    {
        transform.eulerAngles = new Vector3(0.0f, transform.eulerAngles.y, 0.0f);
        controller.enabled = true;
    }
    #endregion AbstractMethods
}
