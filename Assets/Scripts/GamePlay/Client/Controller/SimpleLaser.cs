using UnityEngine;

public class SimpleLaser : MonoBehaviour
{

    [SerializeField]public LineRenderer lineRenderer;
    public float maxDistance = 100f;

    void Update()
    {
        lineRenderer.SetPosition(0, transform.position);
        RaycastHit hit;

        // Cast a ray forward from the hand
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance))
        {
            // If we hit something (like the Canvas), stop the laser there
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            // If we hit nothing, draw a long laser
            lineRenderer.SetPosition(1, transform.position + transform.forward * maxDistance);
        }
    }
}