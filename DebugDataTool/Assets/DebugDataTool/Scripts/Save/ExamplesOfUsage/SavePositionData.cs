public class SavePositionData : SaveDataBase
{
    protected override void Start()
    {
        base.Start();
    }

    public override void OnAddData()
    {
        data.positions.Add(transform.position);
    }
}