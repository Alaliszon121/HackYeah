using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class DissolvePlatforms : MonoBehaviour
{
    public float start = 0f;
    public float end = 1f;
    public float time = 2f;
    private float currentValue;
    private MaterialPropertyBlock propertyBlock;

    private MeshRenderer meshRenderer;
    private MeshRenderer childMeshRenderer; // Dodane pole
    private Collider collider;

    private ParticleSystem particle;
    void Start()
    {
        propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetColor("_Color", Random.ColorHSV());

        meshRenderer = GetComponent<MeshRenderer>();
        if (transform.childCount > 0)
        {
            childMeshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        }

        collider = GetComponent<Collider>();
        particle = GetComponent<ParticleSystem>();
    }

    public IEnumerator LerpFloat()
    {
        if (particle != null)
        {
            var main = particle.main;
            particle.Play();

        }
        collider.enabled = false;
        float elapsed = 0f;

        while (elapsed < time)
        {
            currentValue = Mathf.Lerp(start, end, elapsed / time);
            elapsed += Time.deltaTime;
            Debug.Log("Current Value: " + currentValue);
            //propertyBlock.SetFloat("_DissolveStep", currentValue);
            //meshRenderer.SetPropertyBlock(propertyBlock);
            if (childMeshRenderer != null)
                childMeshRenderer.SetPropertyBlock(propertyBlock);
            yield return null;
        }

        currentValue = end; 
        
        OnLerpFinished();
    }

    void OnLerpFinished()
    {
        Destroy(gameObject);
    }
}
