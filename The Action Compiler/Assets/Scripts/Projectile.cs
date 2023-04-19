using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float timeUntilDestroy = 6f;
    private float speed = 4.5f;


    private void Update()
    {
        if (!InterfaceController.gameIsPaused && Player.cameraInPlace == true)
        {
            timeUntilDestroy -= Time.deltaTime;

            if (timeUntilDestroy <= 0)
            {
                Destroy(gameObject);
            }

            transform.position += Vector3.forward * -1 * speed * Time.deltaTime;

            if (gameObject.name.Substring(0, 9) == "Horizontal".Substring(0, 9))
            {
                transform.localScale += new Vector3((float)(0.1 * Time.timeScale), 0, 0);
            }
        }
    }
}
