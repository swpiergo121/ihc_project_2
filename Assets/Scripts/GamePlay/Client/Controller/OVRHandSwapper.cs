using UnityEngine;
using UnityEngine.EventSystems;

public class OVRHandSwapper : MonoBehaviour
{
    [Header("Settings")]
    public OVRInputModule inputModule;
    public Transform leftHandAnchor;
    public Transform rightHandAnchor;
    //public LineRenderer leftLaser; // Optional: To turn lasers on/off
    //public LineRenderer rightLaser;

    void Update()
    {
        // If player presses Left Trigger, make Left Hand the "Mouse"
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        {
            inputModule.rayTransform = leftHandAnchor;
            SetLasers(true, false);
        }

        // If player presses Right Trigger, make Right Hand the "Mouse"
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            inputModule.rayTransform = rightHandAnchor;
            SetLasers(false, true);
        }
    }

    void SetLasers(bool leftOn, bool rightOn)
    {
        //if (leftLaser) leftLaser.enabled = leftOn;
        //if (rightLaser) rightLaser.enabled = rightOn;
    }
}