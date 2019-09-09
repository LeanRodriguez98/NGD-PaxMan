using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupableObject : MonoBehaviour
{
    public uint pointsValue;
    [HideInInspector] [SerializeField]private Collider2D pickUpZone;

    public void SetCollider()
    {
        pickUpZone = GetComponent<Collider2D>();
    }

    public bool CheckIsPickUped(Transform transform)
    {
        if (pickUpZone == null)
            SetCollider();

        if (pickUpZone.OverlapPoint(transform.position))
            return true;
        return false;
    }

}
