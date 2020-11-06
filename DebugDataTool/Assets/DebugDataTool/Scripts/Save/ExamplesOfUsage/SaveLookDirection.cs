using UnityEngine;

public class SaveLookDirection : SaveDataBase
{
    Camera cam = null;

    protected override void Start()
    {
        base.Start();
        cam = GetComponentInChildren<Camera>();
    }

    public override void OnAddData()
    {
        data.lookDirections.Add(cam.transform.forward);
    }
}