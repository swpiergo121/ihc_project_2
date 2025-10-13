using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DoorTrigger : MonoBehaviour
{
    public GameObject doorHinge; // Now referencing the HINGE object, not the door mesh!
    private bool isOpen = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OpenDoor();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CloseDoor();
        }
    }

    private void OpenDoor()
    {
        if (!isOpen)
        {
            doorHinge.transform.Rotate(0, -75, 0); // Rotating around hinge point
            isOpen = true;
        }
    }

    private void CloseDoor()
    {
        if (isOpen)
        {
            doorHinge.transform.Rotate(0, 75, 0);
            isOpen = false;
        }
    }
}
