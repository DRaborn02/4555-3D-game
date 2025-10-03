using UnityEngine;
using System.Collections.Generic;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance; // Singleton for easy access

    public List<Transform> players = new List<Transform>();
    public Vector3 offset = new Vector3(0, 15f, -10f);
    public float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void LateUpdate()
    {
        if (players.Count == 0) return;

        Vector3 targetPosition = (players.Count == 1) ?
            players[0].position :
            GetCenterPoint();

        targetPosition += offset;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        transform.LookAt(GetCenterPoint()); // optional: camera always points at players
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
