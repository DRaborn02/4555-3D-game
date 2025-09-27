using UnityEngine;

public class Spin : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f; // degrees per second

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}
