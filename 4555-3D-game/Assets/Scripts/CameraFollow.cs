using UnityEngine;
using System.Collections.Generic;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance; // Singleton

    public List<Transform> players = new List<Transform>();
    public Vector3 offset = new Vector3(0, 15f, -10f);
    public float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero;

    [Header("Zoom Settings")]
    public float minZoom = 8f;           // Smallest allowed zoom
    public float padding = 2f;           // Extra space around players
    public float zoomSmooth = 5f;        // Smooth transition

    private Camera cam;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (players.Count == 0) return;

        Vector3 targetPosition = (players.Count == 1) ? players[0].position : GetCenterPoint();
        targetPosition += offset;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        transform.LookAt(GetCenterPoint());

        AdjustZoom();
    }

    void AdjustZoom()
    {
        if (players.Count == 0) return;

        // Build a bounding box around all players
        Bounds bounds = new Bounds(players[0].position, Vector3.zero);
        for (int i = 1; i < players.Count; i++)
            bounds.Encapsulate(players[i].position);

        // Calculate required orthographic size
        float requiredSizeY = bounds.size.y / 2f + padding;
        float requiredSizeX = (bounds.size.x / 2f + padding) / cam.aspect;

        float requiredSize = Mathf.Max(requiredSizeX, requiredSizeY, minZoom);

        // Smooth transition
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, requiredSize, Time.deltaTime * zoomSmooth);
    }

    Vector3 GetCenterPoint()
    {
        if (players.Count == 1) return players[0].position;

        Bounds bounds = new Bounds(players[0].position, Vector3.zero);
        for (int i = 1; i < players.Count; i++)
            bounds.Encapsulate(players[i].position);

        return bounds.center;
    }

    public void RegisterPlayer(Transform player)
    {
        if (!players.Contains(player))
            players.Add(player);
    }

    public void UnregisterPlayer(Transform player)
    {
        if (players.Contains(player))
            players.Remove(player);
    }
}
