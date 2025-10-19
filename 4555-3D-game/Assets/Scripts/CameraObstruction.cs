using System.Collections.Generic;
using UnityEngine;

public class CameraObstruction : MonoBehaviour
{
    public static CameraObstruction Instance;

    [Header("Obstruction Settings")]
    public LayerMask obstructionMask;   // Assign walls layer in inspector

    private HashSet<MeshRenderer> hiddenWalls = new HashSet<MeshRenderer>();
    private List<Transform> players = new List<Transform>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void LateUpdate()
    {
        // Re-enable all previously hidden walls
        foreach (var wall in hiddenWalls)
        {
            if (wall != null) wall.enabled = true;
        }
        hiddenWalls.Clear();

        // Cast rays to each player to check for obstructions
        foreach (Transform player in players)
        {
            if (player == null) continue;

            Vector3 dir = player.position - transform.position;
            Ray ray = new Ray(transform.position, dir);

            RaycastHit[] hits = Physics.RaycastAll(ray, dir.magnitude, obstructionMask);
            foreach (var hit in hits)
            {
                MeshRenderer rend = hit.collider.GetComponent<MeshRenderer>();
                if (rend != null)
                {
                    rend.enabled = false; // Hide wall
                    hiddenWalls.Add(rend);
                }
            }
        }
    }

    // Register/unregister players
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
