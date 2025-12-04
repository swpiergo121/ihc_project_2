using UnityEngine;

public class MenuLaserPointer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float maxDistance = 5.0f;
    public LayerMask uiLayerMask;

    void Start()
    {
        // Auto-get the line renderer if not assigned manually
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();
    }

    void LateUpdate()
    {
        if (lineRenderer == null) return;

        lineRenderer.SetPosition(0, transform.position);

        RaycastHit hit;
        // Check if we hit anything in the UI layer (or default)
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance, uiLayerMask))
        {
            // If we hit something, stop the laser there
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            // If we hit nothing, draw the full length
            lineRenderer.SetPosition(1, transform.position + (transform.forward * maxDistance));
        }
    }
}