using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NPC : MonoBehaviour
{
    [SerializeField] private Color triggeredColor = Color.white;

    private Renderer rend;
    private Color originalColor;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            
            if (rend.material.HasProperty("_BaseColor"))
                originalColor = rend.material.GetColor("_BaseColor");
            else if (rend.material.HasProperty("_Color"))
                originalColor = rend.material.color;
            else
                originalColor = Color.gray;
        }

        
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            // Change NPC’s color
            if (rend != null)
            {
                if (rend.material.HasProperty("_BaseColor"))
                    rend.material.SetColor("_BaseColor", triggeredColor);
                else if (rend.material.HasProperty("_Color"))
                    rend.material.color = triggeredColor;
            }

            //Debug.Log("NPC triggered by Player!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
            if (rend != null)
            {
                if (rend.material.HasProperty("_BaseColor"))
                    rend.material.SetColor("_BaseColor", originalColor);
                else if (rend.material.HasProperty("_Color"))
                    rend.material.color = originalColor;
            }
        }
    }
}
