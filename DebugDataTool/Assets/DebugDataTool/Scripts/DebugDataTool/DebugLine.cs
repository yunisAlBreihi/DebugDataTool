using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DebugLine : MonoBehaviour
{
    [SerializeField, Range(0.0f, 3.0f),
     Tooltip("Simplify the mesh, the higher the number, the simpler it gets.")]
    private float simplifyTolerance = 0.5f;

    private DataHolder data = null;
    private LineRenderer lineRenderer = null;

    public void Create(DataHolder dataHolder)
    {
        data = dataHolder;
        GenerateLine();
    }

    private void GenerateLine()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = data.positions.Count;
        lineRenderer.SetPositions(data.positions.ToArray());
        lineRenderer.Simplify(simplifyTolerance);
    }
}
