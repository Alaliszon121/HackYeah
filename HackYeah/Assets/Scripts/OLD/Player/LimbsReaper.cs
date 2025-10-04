using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LimbsReaper : MonoBehaviour
{
    private Joint joint;
    private Rigidbody joinedRigidbody;
    void Start()
    {
        joint = GetComponent<HingeJoint>();
        if(joint != null) joinedRigidbody = joint.connectedBody;
    }
    void OnJointBreak(float breakForce)
    {
        Debug.Log("A joint has just been broken!, force: " + breakForce);
        if (joinedRigidbody != null) joinedRigidbody.constraints = RigidbodyConstraints.FreezeAll;

        joinedRigidbody.AddComponent<CapsuleCollider>();
        CapsuleCollider collider = joinedRigidbody.GetComponent<CapsuleCollider>();
        collider.isTrigger = true;
        collider.height *= 2f;
        collider.radius *= 2f;
    }
}
