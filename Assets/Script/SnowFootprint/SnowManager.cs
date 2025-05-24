using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class SnowManager : MonoBehaviour
{
    public int maxDeformations = 64; // максимальное количество точек деформации
    private ComputeBuffer deformationBuffer;
    private Material snowMaterial;

    private struct DeformationPoint
    {
        public Vector3 position;
        public float radius;
    }

    private DeformationPoint[] deformations;

    void Start()
    {
        snowMaterial = GetComponent<Renderer>().material;
        deformations = new DeformationPoint[maxDeformations];
        deformationBuffer = new ComputeBuffer(maxDeformations, sizeof(float) * 4);
        snowMaterial.SetBuffer("_DeformationBuffer", deformationBuffer);
    }

    void Update()
    {
        // Обновляем буфер
        deformationBuffer.SetData(deformations);
    }

    public void AddDeformation(Vector3 worldPosition, float radius = 1f)
    {
        for (int i = 0; i < maxDeformations; i++)
        {
            if (deformations[i].radius == 0)
            {
                deformations[i].position = worldPosition;
                deformations[i].radius = radius;
                break;
            }
        }
    }

    void OnDestroy()
    {
        if (deformationBuffer != null)
            deformationBuffer.Release();
    }
}