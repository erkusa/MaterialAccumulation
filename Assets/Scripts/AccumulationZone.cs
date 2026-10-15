using UnityEngine;

public class AccumulationZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TerrainMesh terrain;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Radius Animation")]
    [SerializeField] private float baseRadius = 1.5f;
    [SerializeField] private float amplitude = 0.5f;
    [SerializeField] private float frequency = 1f;
    [SerializeField] private AnimationCurve radiusCurve =
        AnimationCurve.EaseInOut(0f, -1f, 1f, 1f);

    [Header("Accumulation")]
    [SerializeField] private float accumulationSpeed = 1f;
    [SerializeField] private Transform zoneIndicator;

    private float currentRadius;
    private Vector3 previousPosition;
    private float previousRadius;

    
    private void Start()
    {
        previousPosition = transform.position;

        float phase = Mathf.Repeat(
            Time.time * Mathf.Max(0f, frequency),
            1f
        );

        currentRadius = Mathf.Max(
            0.1f,
            baseRadius + amplitude * radiusCurve.Evaluate(phase)
        );

        previousRadius = currentRadius;
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) direction.z += 1f;
        if (Input.GetKey(KeyCode.S)) direction.z -= 1f;
        if (Input.GetKey(KeyCode.A)) direction.x -= 1f;
        if (Input.GetKey(KeyCode.D)) direction.x += 1f;

        direction = direction.normalized;
        transform.position += direction * moveSpeed * dt;

        float halfSize = terrain.Size * 0.5f;

        Vector3 position = transform.position;

        position.x = Mathf.Clamp(
            position.x,
            -halfSize,
            halfSize
        );

        position.z = Mathf.Clamp(
            position.z,
            -halfSize,
            halfSize
        );

        transform.position = position;

        float phase = Mathf.Repeat(Time.time * frequency, 1f);
        currentRadius = Mathf.Max(
            0.1f,
            baseRadius + amplitude * radiusCurve.Evaluate(phase)
        );

        if (Input.GetKey(KeyCode.Space))
        {
            AccumulateAlongPath(dt);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            terrain.ResetTerrain();
        }

        previousPosition = transform.position;
        previousRadius = currentRadius;

        if (zoneIndicator != null)
        {
            zoneIndicator.localScale = new Vector3(
                currentRadius * 2f,
                0.05f,
                currentRadius * 2f
            );
        }
    }

    
    private void AccumulateAlongPath(float dt)
    {
        Vector3 currentPosition = transform.position;

        float distance = Vector3.Distance(
            previousPosition,
            currentPosition
        );

        float spacing = terrain.CellSize * 0.5f;

        int steps = Mathf.Max(
            1,
            Mathf.CeilToInt(distance / spacing)
        );

        float timePerStep = dt / steps;
        bool changed = false;

        for (int step = 0; step < steps; step++)
        {
            float t = (step + 0.5f) / steps;

            Vector3 center = Vector3.Lerp(
                previousPosition,
                currentPosition,
                t
            );

            float radius = Mathf.Lerp(
                previousRadius,
                currentRadius,
                t
            );

            if (AccumulateAt(center, radius, timePerStep))
            {
                changed = true;
            }
        }

        if (changed)
        {
            terrain.UpdateMesh();
        }
    }

    private bool AccumulateAt(
        Vector3 center,
        float radius,
        float dt)
    {
        bool changed = false;
        float radiusSquared = radius * radius;

        int vertexCount =
            (terrain.Resolution + 1) * (terrain.Resolution + 1);

        for (int i = 0; i < vertexCount; i++)
        {
            Vector3 vertex = terrain.GetVertexPosition(i);

            float dx = vertex.x - center.x;
            float dz = vertex.z - center.z;

            float distanceSquared = dx * dx + dz * dz;

            if (distanceSquared >= radiusSquared)
                continue;

            float hemisphereFactor = Mathf.Sqrt(
                1f - distanceSquared / radiusSquared
            );

            float amount =
                accumulationSpeed * dt * hemisphereFactor;

            terrain.AddMaterial(i, amount);
            changed = true;
        }

        return changed;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            transform.position,
            Application.isPlaying ? currentRadius : baseRadius
        );
    }
}
