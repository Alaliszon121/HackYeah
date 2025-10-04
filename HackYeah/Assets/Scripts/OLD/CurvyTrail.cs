using UnityEngine;
using System;

[RequireComponent(typeof(LineRenderer))]
public class CurvyTrail : MonoBehaviour
{
    public int segmentCount = 10;    
    public float trailLength = 3f;   
    public float swayAmount = 1f;    
    public float curveStrength = 0.5f; 
    public float smoothSpeed = 5f;

    private LineRenderer line;
    private float targetOffsetX = 0f;
    private float currentOffsetX = 0f;

    void OnEnable()
    {
        GameEvents.OnGravityInverted += HandleGravityInverted;
    }

    void OnDisable()
    {
        GameEvents.OnGravityInverted -= HandleGravityInverted;
    }

    private void HandleGravityInverted(bool isReversed)
    {
        if (isReversed) trailLength = -2f;
        else trailLength = 2f;
    }

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = segmentCount;
    }

    void Update()
    {
        float input = Input.GetAxisRaw("Horizontal");
        targetOffsetX = -input * swayAmount;
        currentOffsetX = Mathf.Lerp(currentOffsetX, targetOffsetX, Time.deltaTime * smoothSpeed);

        Vector3 startPos = transform.position;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1); 
            float y = t * trailLength;

            float x = Mathf.Sin(t * Mathf.PI * 0.5f) * currentOffsetX * curveStrength;

            Vector3 pos = startPos + new Vector3(x, y, 0f);
            line.SetPosition(i, pos);
        }
    }
}