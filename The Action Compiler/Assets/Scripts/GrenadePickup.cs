using UnityEngine;

public class GrenadePickup : MonoBehaviour
{
    private float timeUntilDestroy = 15f;

    private void Start()
    {
        Player.AddSpawnedPickup?.Invoke(gameObject);
    }

    private void OnDestroy()
    {
        Player.RemoveSpawnedPickup?.Invoke(gameObject);
    }

    void Update()
    {
        if (!InterfaceController.gameIsPaused)
        {
            timeUntilDestroy -= Time.deltaTime;

            if (timeUntilDestroy <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
