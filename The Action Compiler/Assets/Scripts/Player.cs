using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Player : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Animator gunAnimator;
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private GameObject grenadePickupPrefab;
    [SerializeField] private GameObject notificationTextObject;
    [SerializeField] private List<Image> bulletIconImages = new List<Image>();
    [SerializeField] private List<Image> grenadeIconImages = new List<Image>();
    [SerializeField] private ParticleSystem[] gunshotParticles;

    private Text notificationText;

    private List<GameObject> spawnedPickups = new List<GameObject>();
    public static Action<GameObject> AddSpawnedPickup;
    public static Action<GameObject> RemoveSpawnedPickup;
    public static Func<bool, bool> CheckIfBossIsAlive;
    public static Action DisplayInvalidActionCall;
    public static Action RestoreHealthToFull;

    private int playerHealth = 100;
    private int magazineSize = 3;
    private int shotsLeft;
    private int numOfGrenades = 0;

    private float moveSpeed = 6f;
    private Vector3 positionToLerpTo;
    private bool isMoving = false;

    private Rigidbody playerRb;
    private float jumpForce = 7f;

    private float timeUntilSpawnPickup;
    private bool bossIsAlive = true;

    public static bool cameraInPlace = false;

    private void OnEnable()
    {
        ActionInventory.Left += Left;
        ActionInventory.Right += Right;
        ActionInventory.Jump += Jump;
        ActionInventory.Shoot += Shoot;
        ActionInventory.Grenade += Grenade;
        ActionInventory.Reload += Reload;
        ActionInventory.Pickup += PickUpItem;

        AddSpawnedPickup += AddToSpawnedPickups;
        RemoveSpawnedPickup += RemoveFromSpawnedPickups;
        CheckIfBossIsAlive += CheckBossStatus;
        DisplayInvalidActionCall += InvalidActionCallNotification;
        RestoreHealthToFull += RestoreHealth;
    }

    private void OnDisable()
    {
        ActionInventory.Left -= Left;
        ActionInventory.Right -= Right;
        ActionInventory.Jump -= Jump;
        ActionInventory.Shoot -= Shoot;
        ActionInventory.Grenade -= Grenade;
        ActionInventory.Reload -= Reload;
        ActionInventory.Pickup -= PickUpItem;

        AddSpawnedPickup -= AddToSpawnedPickups;
        RemoveSpawnedPickup -= RemoveFromSpawnedPickups;
        CheckIfBossIsAlive -= CheckBossStatus;
        DisplayInvalidActionCall -= InvalidActionCallNotification;
        RestoreHealthToFull -= RestoreHealth;
    }

    private void Awake()
    {
        shotsLeft = magazineSize;
        playerRb = gameObject.GetComponent<Rigidbody>();
        timeUntilSpawnPickup = UnityEngine.Random.Range(10f, 20f);
        notificationText = notificationTextObject.GetComponent<Text>();

        for (int i = 3; i < bulletIconImages.Count; i++)
        {
            bulletIconImages[i].color = new Color32(255, 255, 255, 25);
        }

        for (int i = 0; i < grenadeIconImages.Count; i++)
        {
            grenadeIconImages[i].color = new Color32(255, 255, 255, 25);
        }
    }

    private void Update()
    {
        if (!InterfaceController.gameIsPaused)
        {
            MoveCameraToPlayerFromMenu();

            CalculateWhereToLookAt();

            SmoothMove();

            CountdownToSpawnPickup();
        }
        else
        {
            MoveCameraToMenuFromPlayer();
        }
    }

    private void Left(string typedLine)
    {
        if (transform.position.x >= -0.1f && isMoving == false)
        {
            DetermineNewLane(typedLine);
        }
        else
        {
            notificationText.text = "Can't move left!";
            notificationTextObject.SetActive(false);
            notificationTextObject.SetActive(true);
        }
    }
    private void Right(string typedLine)
    {
        if (transform.position.x <= 0.1 && isMoving == false)
        {
            DetermineNewLane(typedLine);
        }
        else
        {
            notificationText.text = "Can't move right!";
            notificationTextObject.SetActive(false);
            notificationTextObject.SetActive(true);
        }
    }
    private void Jump(string typedLine)
    {
        if (transform.position.y < 1.05f && isMoving == false)
        {
            if (typedLine == "Action.Jump();")
            {
                playerRb.AddForce(Vector3.up * (jumpForce + 1.75f), ForceMode.Impulse);
            }
            else if (typedLine == "Action.Jump?.Invoke();")
            {
                playerRb.AddForce(Vector3.up * (jumpForce + 3f), ForceMode.Impulse);

                StartCoroutine(SlowTime());
            }
            else
            {
                playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
        else
        {
            notificationText.text = "Can't jump!";
            notificationTextObject.SetActive(false);
            notificationTextObject.SetActive(true);
        }
    }


    private void Shoot(string typedLine)
    {
        if (shotsLeft > 0 && bossIsAlive == true)
        {
            shotsLeft--;

            bulletIconImages[shotsLeft].color = new Color32(255, 255, 255, 25);

            foreach (ParticleSystem particles in gunshotParticles)
            {
                particles.Play();
            }

            if (shotsLeft == 0)
            {
                gunAnimator.SetBool("Emptied", true);
            }
            else
            {
                gunAnimator.SetTrigger("Recoil");
            }

            if (typedLine == "Action.Shoot();")
            {
                int damage = 20;
                Boss.BossTakeDamage?.Invoke(damage);
            }
            else if (typedLine == "Action.Shoot?.Invoke();")
            {
                int damage = 30;
                Boss.BossTakeDamage?.Invoke(damage);
            }
            else
            {
                int damage = 10;
                Boss.BossTakeDamage?.Invoke(damage);
            }
        }
        else if (shotsLeft == 0)
        {
            notificationText.text = "Need to reload!";
            notificationTextObject.SetActive(false);
            notificationTextObject.SetActive(true);
        }
        else
        {
            notificationText.text = "Nothing to shoot!";
            notificationTextObject.SetActive(false);
            notificationTextObject.SetActive(true);
        }
    }

    private void Grenade(string typedLine)
    {
        if (numOfGrenades > 0 && bossIsAlive == true)
        {
            numOfGrenades--;

            grenadeIconImages[numOfGrenades].color = new Color32(255, 255, 255, 25);

            Vector3 velocity = transform.forward * 1.0f + transform.up * 1.0f;  // throw at 45 degree angle

            float angleLeftRight;

            if (transform.position.x < 0)
            {
                angleLeftRight = UnityEngine.Random.Range(0f, 10f);
            }
            else if (transform.position.x > 0)
            {
                angleLeftRight = UnityEngine.Random.Range(-10f, 0f);
            }
            else
            {
                angleLeftRight = UnityEngine.Random.Range(-5f, 5f);
            }

            Quaternion rotation = Quaternion.Euler(0, angleLeftRight, 0);   // change the throw angle, currently slight variation

            Vector3 trueVelocity = rotation * velocity * 9f;


            GameObject grenade = Instantiate(grenadePrefab, new Vector3(transform.position.x - 0.5f, 0.8f, -10.5f), Quaternion.Euler(-90, 0, 0));

            Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();

            grenadeRb.velocity = trueVelocity;
            grenadeRb.AddTorque(Vector3.right, ForceMode.Impulse);


            if (typedLine == "Action.Grenade();")
            {
                int damage = 50;
                GrenadeController.GetDamage?.Invoke(damage);
            }
            else if (typedLine == "Action.Grenade?.Invoke();")
            {
                int damage = 70;
                GrenadeController.GetDamage?.Invoke(damage);
            }
            else
            {
                int damage = 30;
                GrenadeController.GetDamage?.Invoke(damage);
            }
        }
        else if (numOfGrenades == 0)
        {
            notificationText.text = "No grenades left!";
            notificationTextObject.SetActive(false);
            notificationTextObject.SetActive(true);
        }
        else
        {
            notificationText.text = "Nothing to blow up!";
            notificationTextObject.SetActive(false);
            notificationTextObject.SetActive(true);
        }
    }

    private void Reload(string typedLine)
    {
        gunAnimator.SetTrigger("Reload");
        gunAnimator.SetBool("Emptied", false);

        if (typedLine == "Action.Reload();")
        {
            shotsLeft = magazineSize + 2;
        }
        else if (typedLine == "Action.Reload?.Invoke();")
        {
            shotsLeft = magazineSize + 4;
        }
        else
        {
            shotsLeft = magazineSize;
        }

        for (int i = 0; i < 7; i++)
        {
            if (i < 3)
            {
                bulletIconImages[i].color = new Color32(255, 255, 255, 255);
            }
            else if (i < 5 && shotsLeft > 3)
            {
                bulletIconImages[i].color = new Color32(224, 133, 69, 255);
            }
            else if (i < 7 && shotsLeft > 5)
            {
                bulletIconImages[i].color = new Color32(199, 70, 70, 255);
            }
            else if (i >= shotsLeft)
            {
                bulletIconImages[i].color = new Color32(255, 255, 255, 25);
            }
        }
    }

    private void PickUpItem(string typedLine)
    {
        if (spawnedPickups.Count > 0)
        {
            if (typedLine == "Action.Pickup();")
            {
                if (spawnedPickups[0].name.Substring(0, 6) == "Grenade".Substring(0, 6))
                {
                    numOfGrenades += 2;

                    if (numOfGrenades > 3)
                    {
                        numOfGrenades = 3;

                        notificationText.text = "Grenades maxed out!";
                        notificationTextObject.SetActive(false);
                        notificationTextObject.SetActive(true);
                    }
                    else
                    {
                        notificationText.text = "Grenades x2";
                        notificationTextObject.SetActive(false);
                        notificationTextObject.SetActive(true);
                    }

                    for (int i = 0; i < numOfGrenades; i++)
                    {
                        grenadeIconImages[i].color = new Color32(255, 255, 255, 255);
                    }
                }
            }
            else if (typedLine == "Action.Pickup?.Invoke();")
            {
                if (spawnedPickups[0].name.Substring(0, 6) == "Grenade".Substring(0, 6))
                {
                    numOfGrenades += 3;

                    if (numOfGrenades > 3)
                    {
                        numOfGrenades = 3;
                    }

                    notificationText.text = "Grenades maxed out!";
                    notificationTextObject.SetActive(false);
                    notificationTextObject.SetActive(true);

                    for (int i = 0; i < numOfGrenades; i++)
                    {
                        grenadeIconImages[i].color = new Color32(255, 255, 255, 255);
                    }
                }
            }
            else
            {
                if (spawnedPickups[0].name.Substring(0, 6) == "Grenade".Substring(0, 6))
                {
                    numOfGrenades++;

                    if (numOfGrenades > 3)
                    {
                        numOfGrenades = 3;

                        notificationText.text = "Grenades maxed out!";
                        notificationTextObject.SetActive(false);
                        notificationTextObject.SetActive(true);
                    }
                    else
                    {
                        notificationText.text = "Grenades++;";
                        notificationTextObject.SetActive(false);
                        notificationTextObject.SetActive(true);
                    }

                    for (int i = 0; i < numOfGrenades; i++)
                    {
                        grenadeIconImages[i].color = new Color32(255, 255, 255, 255);
                    }
                }
            }

            Destroy(spawnedPickups[0]);
            spawnedPickups.Remove(spawnedPickups[0]);
        }
        else
        {
            notificationText.text = "Nothing to pick up!";
            notificationTextObject.SetActive(false);
            notificationTextObject.SetActive(true);
        }
    }


    private void TakeDamage()
    {
        if (bossIsAlive == true)
        {
            playerHealth -= (int)Boss.ProjectileDamage?.Invoke();

            VolumeController.ApplyDamageEffects?.Invoke();

            if (playerHealth <= 0)
            {
                InterfaceController.gameIsPaused = true;
                InterfaceController.ReloadCurrentScene?.Invoke();
            }
        }
    }

    private void RestoreHealth()
    {
        playerHealth = 100;
    }


    private void DetermineNewLane(string typedLine)
    {
        if (typedLine == "Left();")
        {
            positionToLerpTo = new Vector3(transform.position.x - 4.275f, transform.position.y, transform.position.z);

            playerRb.isKinematic = true;
            isMoving = true;
        }
        else if (typedLine == "Action.Left();" || typedLine == "Action.Left?.Invoke();")
        {
            if (transform.position.x >= 3.4f)
            {
                positionToLerpTo = new Vector3(transform.position.x - 8.55f, transform.position.y, transform.position.z);
            }
            else
            {
                positionToLerpTo = new Vector3(transform.position.x - 4.275f, transform.position.y, transform.position.z);
            }

            playerRb.isKinematic = true;
            isMoving = true;
        }

        else if (typedLine == "Right();")
        {
            positionToLerpTo = new Vector3(transform.position.x + 4.275f, transform.position.y, transform.position.z);

            playerRb.isKinematic = true;
            isMoving = true;
        }
        else if (typedLine == "Action.Right();" || typedLine == "Action.Right?.Invoke();")
        {
            if (transform.position.x <= -3.4f)
            {
                positionToLerpTo = new Vector3(transform.position.x + 8.55f, transform.position.y, transform.position.z);
            }
            else
            {
                positionToLerpTo = new Vector3(transform.position.x + 4.275f, transform.position.y, transform.position.z);
            }

            playerRb.isKinematic = true;
            isMoving = true;
        }
        else
        {
            playerRb.isKinematic = false;
            isMoving = false;
        }

        if (typedLine == "Action.Left?.Invoke();" || typedLine == "Action.Right?.Invoke();")
        {
            StartCoroutine(SlowTime());
        }
    }

    private void SmoothMove()
    {
        if (isMoving == true)
        {
            if (playerRb.isKinematic == true)
            {
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, positionToLerpTo, moveSpeed * Time.deltaTime);

                transform.position = smoothedPosition;
            }

            if (Mathf.Abs(transform.position.x - positionToLerpTo.x) <= 0.01f)
            {
                playerRb.isKinematic = false;
            }
            if (playerRb.isKinematic == false && transform.position.y < 1.05f)
            {
                if (transform.position.x < -4)
                {
                    transform.position = new Vector3(-4.275f, 1, transform.position.z);
                    isMoving = false;
                }
                else if (transform.position.x > 4)
                {
                    transform.position = new Vector3(4.275f, 1, transform.position.z);
                    isMoving = false;
                }
                else
                {
                    transform.position = new Vector3(0, 1, transform.position.z);
                    isMoving = false;
                }
            }
        }
    }


    private void CountdownToSpawnPickup()
    {
        if (spawnedPickups.Count == 0 && bossIsAlive == true)
        {
            timeUntilSpawnPickup -= Time.deltaTime;

            if (timeUntilSpawnPickup <= 0)
            {
                Instantiate(grenadePickupPrefab, new Vector3(UnityEngine.Random.Range(-4.3f, 4.3f), 0.1f, UnityEngine.Random.Range(-6.2f, -2.2f)), grenadePickupPrefab.transform.rotation);

                timeUntilSpawnPickup = timeUntilSpawnPickup = UnityEngine.Random.Range(10f, 20f);
            }
        }
    }

    private void AddToSpawnedPickups(GameObject pickup)
    {
        spawnedPickups.Add(pickup);
    }

    private void RemoveFromSpawnedPickups(GameObject pickup)
    {
        if (spawnedPickups.Contains(pickup))
        {
            spawnedPickups.Remove(pickup);
        }
    }


    private bool CheckBossStatus(bool bossStatus)
    {
        bossIsAlive = bossStatus;
        return bossIsAlive;
    }
    private void CalculateWhereToLookAt()
    {
        if (bossIsAlive == true && cameraInPlace == true)
        {
            transform.LookAt((Vector3)(Boss.BossLocation?.Invoke()));
        }
        else if (cameraInPlace == true)
        {
            transform.LookAt(new Vector3(0, 1.75f, 12));
        }
    }

    private void InvalidActionCallNotification()
    {
        notificationText.text = "Invalid action call!";
        notificationTextObject.SetActive(false);
        notificationTextObject.SetActive(true);
    }


    private void MoveCameraToPlayerFromMenu()
    {
        if (cameraInPlace == false)
        {
            Vector3 smoothedPosition = Vector3.Lerp(playerCamera.transform.position, new Vector3(transform.position.x, 2, transform.position.z), moveSpeed * Time.deltaTime);

            playerCamera.transform.position = smoothedPosition;

            if (Mathf.Abs(playerCamera.transform.position.y - 2) <= 0.01f)
            {
                playerCamera.transform.position = new Vector3(transform.position.x, 2, transform.position.z);
                cameraInPlace = true;

                InterfaceController.TogglePlayerCanvas?.Invoke();
            }
        }
    }
    private void MoveCameraToMenuFromPlayer()
    {
        if (InterfaceController.gameIsPaused == true)
        {
            Vector3 smoothedPosition = Vector3.Lerp(playerCamera.transform.position, new Vector3(transform.position.x, 17, transform.position.z), moveSpeed * Time.deltaTime);

            playerCamera.transform.position = smoothedPosition;

            if (Mathf.Abs(playerCamera.transform.position.y - 17) <= 0.01f)
            {
                playerCamera.transform.position = new Vector3(transform.position.x, 17, transform.position.z);

                InterfaceController.ToggleMenuCanvas?.Invoke();
            }
        }
    }

    private IEnumerator SlowTime()
    {
        Time.timeScale = 0.5f;

        notificationText.text = "Time: x0.5f";
        notificationTextObject.SetActive(false);
        notificationTextObject.SetActive(true);

        yield return new WaitForSecondsRealtime(5f);

        Time.timeScale = 1f;

        notificationText.text = "Time: x1.0f";
        notificationTextObject.SetActive(false);
        notificationTextObject.SetActive(true);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Projectile")
        {
            TakeDamage();
        }
    }
}
