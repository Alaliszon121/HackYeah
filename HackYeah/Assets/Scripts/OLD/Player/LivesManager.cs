using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BodyParts
{
    public GameObject bodyPart;
    public Joint joint;
    public Vector3 originalPosition;
    public Quaternion originalRotation;

    // Dodaj te pola:
    public Vector3 anchor;
    public Vector3 axis;
    public bool useSpring;
    public JointSpring spring;
}

public class LivesManager : MonoBehaviour
{
    public List<BodyParts> bodyParts;
    public PlayerMovement playerMovement;
    public Rigidbody playerBody;
    public ScreenShake screenShake;

    public AudioSource pick;
    public AudioSource hurt;

    void Start()
    {
        var joints = playerBody.GetComponents<HingeJoint>();

        foreach (var part in bodyParts)
        {
            part.originalPosition = playerBody.transform.InverseTransformPoint(part.bodyPart.transform.position);
            part.originalRotation = Quaternion.Inverse(playerBody.transform.rotation) * part.bodyPart.transform.rotation;

            foreach (var hinge in joints)
            {
                if (hinge.connectedBody == part.bodyPart.GetComponent<Rigidbody>())
                {
                    part.anchor = hinge.anchor;
                    part.axis = hinge.axis;
                    part.useSpring = hinge.useSpring;
                    part.spring = hinge.spring;
                    part.joint = hinge;
                    break;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided with: " + collision.gameObject.tag);
        if (collision.gameObject.CompareTag("Damagable"))
        {
            hurt.Play();
            //if(screenShake != null) screenShake.Shake(0.5f, 5f);
            Debug.Log("Hit by Damagable");
            //if (collision.gameObject.TryGetComponent<DissolvePlatforms>(out DissolvePlatforms dissolvable)) 
            {
                //StartCoroutine(dissolvable.LerpFloat());
            }
            if (bodyParts.Count > 0)
            {
                int lastIndex = bodyParts.Count - 1;
                BodyParts partToRemove = bodyParts[lastIndex];

                // Wy��cz CurvyTrail na wszystkich dzieciach tej ko�czyny
                if (lastIndex > 0) // pomijamy pierwsz� ko�czyn� w li�cie
                {
                    foreach (Transform child in partToRemove.bodyPart.transform)
                    {
                        var curvyTrail = child.GetComponent<CurvyTrail>();
                        var lineRenderer = child.GetComponent<LineRenderer>();
                        if (curvyTrail != null) { curvyTrail.enabled = false;
                            lineRenderer.enabled = false;
                        }
                            

                    }
                }

                var limbTrigger = partToRemove.bodyPart.AddComponent<LimbTrigger>();
                partToRemove.bodyPart.gameObject.GetComponent<ArmMover>().isRiped = true;
                limbTrigger.livesManager = this;
                limbTrigger.limbData = partToRemove;
                partToRemove.joint.connectedBody = null;
                Destroy(partToRemove.joint);

                var rb = partToRemove.bodyPart.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.constraints = RigidbodyConstraints.FreezeAll;

                CapsuleCollider trigger = partToRemove.bodyPart.AddComponent<CapsuleCollider>();
                trigger.isTrigger = true;

                bodyParts.RemoveAt(lastIndex);

                StartCoroutine(ChangeLayerWithDelay(partToRemove.bodyPart, "Outlines", 1.5f));
            }
            if (bodyParts.Count == 0)
            {
                if (playerMovement != null)
                {
                    playerMovement.enabled = false;
                }
                Die();
                Debug.Log("Game Over!");
            }
        }
         if (collision.gameObject.CompareTag("ceilling"))
        {
            Die();
        }
    }

    private IEnumerator ChangeLayerWithDelay(GameObject obj, string layerName, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.layer = LayerMask.NameToLayer(layerName);
    }

    public void AttachLimb(BodyParts limb)
    {
        pick.Play();
        ParticleSystem particle = limb.bodyPart.gameObject.GetComponent<ParticleSystem>();
        if(particle != null)
        {
            var main = particle.main;
            particle.Play();
        }

        limb.bodyPart.transform.position = playerBody.transform.TransformPoint(limb.originalPosition);
        limb.bodyPart.transform.rotation = playerBody.transform.rotation * limb.originalRotation;

        var rb = limb.bodyPart.GetComponent<Rigidbody>();
        if (rb != null)
            rb.constraints = RigidbodyConstraints.None;

        var trigger = limb.bodyPart.GetComponent<CapsuleCollider>();
        if (trigger != null)
            Destroy(trigger);

        var limbTrigger = limb.bodyPart.GetComponent<LimbTrigger>();
        if (limbTrigger != null)
            Destroy(limbTrigger);

        var joint = playerBody.gameObject.AddComponent<HingeJoint>();
        joint.connectedBody = rb;
        joint.anchor = limb.anchor;
        joint.axis = limb.axis;
        joint.useSpring = limb.useSpring;
        joint.spring = limb.spring;
        limb.joint = joint;

        // Zmień layer na "BodyParts" przy podczepianiu
        limb.bodyPart.layer = LayerMask.NameToLayer("BodyParts");
        limb.bodyPart.gameObject.GetComponent<ArmMover>().isRiped = false;
        // Włącz CurvyTrail na wszystkich dzieciach tej kołczyny
        int limbIndex = bodyParts.Count; // po dodaniu limb do bodyParts
        if (limbIndex > 0)
        {
            foreach (Transform child in limb.bodyPart.transform)
            {
                var curvyTrail = child.GetComponent<CurvyTrail>();
                var lineRenderer = child.GetComponent<LineRenderer>();
                if (curvyTrail != null) { lineRenderer.enabled = true;
                    curvyTrail.enabled = true;
                }
                    
            }
        }

        bodyParts.Add(limb);
        Debug.Log("Limb reattached!");
    }
    public void Die()
    {
        UIManager.instance.loseScreenPanel.SetActive(true);
        ScoreManager.instance.ResetScore();
        Time.timeScale = 0f;
    }
}

public class LimbTrigger : MonoBehaviour
{
    public LivesManager livesManager;
    public BodyParts limbData;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            livesManager.AttachLimb(limbData);
        }
    }
}
