public class SavePositionData : SaveDataBase
{
    public override void OnAddData()
    {
        data.positions.Add(transform.position);
    }
}