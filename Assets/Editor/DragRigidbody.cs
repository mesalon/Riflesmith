using UnityEngine;

public class DragRigidbodyWithForce : MonoBehaviour
{
    private Camera mainCamera;
    private Rigidbody selectedRigidbody;
    private Vector3 offset;
    private float zDepth;
    private bool isDragging;

    [SerializeField] float dragForce = 10f; // Adjust the force strength for dragging

    void Start()
    {
        mainCamera = GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.rigidbody != null)
                {
                    selectedRigidbody = hit.rigidbody;
                    zDepth = mainCamera.WorldToScreenPoint(selectedRigidbody.position).z;
                    Vector3 screenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, zDepth);
                    offset = selectedRigidbody.position - mainCamera.ScreenToWorldPoint(screenPoint);
                    isDragging = true;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            selectedRigidbody = null;
            isDragging = false;
        }
    }

    void FixedUpdate()
    {
        if (isDragging && selectedRigidbody != null)
        {
            Vector3 screenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, zDepth);
            Vector3 targetPosition = mainCamera.ScreenToWorldPoint(screenPoint) + offset;
            Vector3 forceDirection = (targetPosition - selectedRigidbody.position) * dragForce;

            selectedRigidbody.AddForce(forceDirection, ForceMode.Force);
        }
    }
}