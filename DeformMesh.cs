using UnityEngine;
using System.Collections.Generic;

public class DeformableCar : MonoBehaviour
{
    
    // car Mesh
    public MeshFilter carMeshFilter;

    // deform Force
    public float deformationForce = 100f;

    // deformation Smoothness
    public float deformationSmoothness = 0.5f;

    [Range(0.05f, 3f)]
    // affected area
    public float deformDist = 2f;

    // transform List
    public List<Transform> deformationTargets;

    // Vertex deformation Limit
    public int maxDeformationsPerVertex = 3;

    private Vector3[] originalVertices;
    private Dictionary<int, int> deformationCounts;

    void Start()
    {
        if (carMeshFilter != null)
        {
            originalVertices = carMeshFilter.mesh.vertices;
            deformationCounts = new Dictionary<int, int>(originalVertices.Length);
        }
    }

    void Update()
    {
        //for testing
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetDeformation();
        }
    }

    // apply deformation
    public void ApplyDeformation(Vector3 contactPoint, Vector3 contactNormal)
    {
        if (deformationTargets == null || deformationTargets.Count == 0)
            return;

        Transform closestTarget = FindClosestTarget(contactPoint);
        Mesh mesh = carMeshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3 localContactPoint = carMeshFilter.transform.InverseTransformPoint(contactPoint);
        Vector3 localDeformationTarget = carMeshFilter.transform.InverseTransformPoint(closestTarget.position);

        for (int i = 0; i < vertices.Length; i++)
        {
            // checks if max deformation reached
            if (deformationCounts.ContainsKey(i) && deformationCounts[i] >= maxDeformationsPerVertex)
            {
                continue;
            }

            float distanceToContact = Vector3.Distance(vertices[i], localContactPoint);
            if (distanceToContact <= deformDist*.001f)
            {
                float deformationAmount = Mathf.Clamp01(1 - distanceToContact / deformationSmoothness);

                // Calculate deformation direction based on the closest transform
                Vector3 deformationDirection = (localDeformationTarget - vertices[i]).normalized;

                // Apply deformation towards the closest transform
                vertices[i] += deformationDirection * deformationAmount * deformationForce/100 * Time.deltaTime;

                // Update deformation count for this vertex
                if (!deformationCounts.ContainsKey(i))
                {
                    deformationCounts[i] = 0;
                }
                deformationCounts[i]++;
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }

    // Method to find the closest transform to the contact point
    private Transform FindClosestTarget(Vector3 contactPoint)
    {
        Transform closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform target in deformationTargets)
        {
            float distance = Vector3.Distance(contactPoint, target.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }

        return closestTarget;
    }

    // Method to handle collisions
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody != null)
        {
            Vector3 collisionPoint = collision.contacts[0].point;
            Vector3 collisionNormal = collision.contacts[0].normal;

            // Apply deformation to the car
            ApplyDeformation(collisionPoint, collisionNormal);
        }
    }

    // Method to reset deformation
    public void ResetDeformation()
    {
        if (carMeshFilter != null && originalVertices != null)
        {
            Mesh mesh = carMeshFilter.mesh;
            mesh.vertices = originalVertices;
            mesh.RecalculateNormals();

            // Reset deformation counts
            deformationCounts.Clear();
        }
    }
}
