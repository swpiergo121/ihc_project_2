using UnityEngine;

/// <summary>
/// This script is just a "tag" to identify the discard zone.
/// Place this on your 'DiscardIndicator' GameObject.
/// That GameObject MUST also have a Collider set to 'isTrigger = true'.
/// </summary>
public class DiscardArea : MonoBehaviour
{
    // This script can be empty. Its only purpose
    // is to be identified by the VRHandTile script.
}