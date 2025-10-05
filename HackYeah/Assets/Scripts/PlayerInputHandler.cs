using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private GameInputActions controls;
    private NoteSpawner noteSpawner;

    private Material[] originalMaterials;
    public Renderer[] targetRenderers;

    private void Awake()
    {
        controls = new GameInputActions();
        noteSpawner = FindObjectOfType<NoteSpawner>();

        controls.Player.PlayA.performed += ctx => noteSpawner.TryHit(0);
        controls.Player.PlayB.performed += ctx => noteSpawner.TryHit(1);
        controls.Player.PlayC.performed += ctx => noteSpawner.TryHit(2);
        controls.Player.PlayD.performed += ctx => noteSpawner.TryHit(3);

        // Save original materials
        originalMaterials = new Material[targetRenderers.Length];
        for (int i = 0; i < targetRenderers.Length; i++)
            originalMaterials[i] = targetRenderers[i].material;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();
}
