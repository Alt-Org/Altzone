using UnityEngine;

public class ItemPopUp : MonoBehaviour
{
    // The popup object that will appear when the item is clicked
    public GameObject popupObject;

    // Update is called once per frame
    void Update()
    {
        // Check for mouse click
        if (Input.GetMouseButtonDown(0))
        {
            // Cast a ray from the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the ray hits this object
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
            {
                // Show the popup object
                if (popupObject != null)
                {
                    popupObject.SetActive(true);
                }
            }
            else
            {
                // Hide the popup object if it's not clicked on
                if (popupObject != null)
                {
                    popupObject.SetActive(false);
                }
            }
        }
    }
}
