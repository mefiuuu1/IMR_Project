using UnityEngine;

public class SwordHandler : MonoBehaviour
{
    [Header("References")]
    public Transform sword;
    public Transform handBone;
    public Transform sheathPosition;

    [Header("Sword Position in Hand")]
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;

    [Header("State")]
    public bool swordInHand = false;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;

    void Start()
    {
        if (sword != null)
        {
            originalPosition = sword.localPosition;
            originalRotation = sword.localRotation;
            originalParent = sword.parent;

            if (sheathPosition == null)
            {
                GameObject sheath = new GameObject("SheathPosition");
                sheath.transform.SetParent(originalParent);
                sheath.transform.localPosition = originalPosition;
                sheath.transform.localRotation = originalRotation;
                sheathPosition = sheath.transform;
            }
        }
    }

    public void GrabSword()
    {
        if (sword == null || handBone == null) return;

        sword.SetParent(handBone);
        sword.localPosition = positionOffset;
        sword.localRotation = Quaternion.Euler(rotationOffset);

        swordInHand = true;

        Debug.Log("Sword grabbed!");
    }

    public void SheathSword()
    {
        if (sword == null) return;

        if (sheathPosition != null)
        {
            sword.SetParent(sheathPosition.parent);
            sword.localPosition = originalPosition;
            sword.localRotation = originalRotation;
        }
        else if (originalParent != null)
        {
            sword.SetParent(originalParent);
            sword.localPosition = originalPosition;
            sword.localRotation = originalRotation;
        }

        swordInHand = false;

        Debug.Log("Sword sheathed!");
    }

    [ContextMenu("Test Grab Sword")]
    void TestGrab()
    {
        GrabSword();
    }

    [ContextMenu("Test Sheath Sword")]
    void TestSheath()
    {
        SheathSword();
    }
}
