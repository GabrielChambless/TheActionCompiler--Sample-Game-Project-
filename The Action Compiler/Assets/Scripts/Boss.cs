using System.Collections;
using UnityEngine;
using System;
using TMPro;

public class Boss : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject horizontalProjectilePrefab;
    [SerializeField] private TMP_Text monitorScreenText;
    [SerializeField] private GameObject lennyFace;
    [SerializeField] private GameObject bossHealthPercentage;

    public static Action<int> BossTakeDamage;
    public static Func<int> ProjectileDamage;
    public static Func<Vector3> BossLocation;

    private int bossHealth = 100;
    private TMP_Text bossHealthText;

    private int bossLaneNumber = 2;
    private int newBossLaneNumber;
    private Vector3 positionToLerpTo;
    private float moveSpeed = 4f;

    private bool isMoving = false;
    private float moveCooldownTime = 2f;
    private float timeUntilCanMove;

    private float shootCooldownTime = 2f;
    private float timeUntilShoot;

    private int randomNumber;
    private bool monitorIsTyping = false;

    private void OnEnable()
    {
        BossTakeDamage += TakeDamage;
        ProjectileDamage += DealDamage;
        BossLocation += ShareLocation;
    }

    private void OnDisable()
    {
        BossTakeDamage -= TakeDamage;
        ProjectileDamage -= DealDamage;
        BossLocation -= ShareLocation;

        Player.CheckIfBossIsAlive?.Invoke(false);
    }


    private void Awake()
    {
        bossHealthText = bossHealthPercentage.GetComponent<TMP_Text>();

        timeUntilCanMove = moveCooldownTime;
        timeUntilShoot = shootCooldownTime;
    }

    private void Update()
    {
        if (!InterfaceController.gameIsPaused && Player.cameraInPlace == true)
        {
            if (timeUntilCanMove == moveCooldownTime)
            {
                randomNumber = UnityEngine.Random.Range(1, 4);  // generate a random number for determing which lane to move to and which kind of projectile to shoot
            }

            ProcessMonitorText();

            if (isMoving == false)
            {
                timeUntilCanMove -= Time.deltaTime;
                timeUntilShoot -= Time.deltaTime;
            }

            DetermineNewLane(moveCooldownTime, randomNumber);

            SmoothMove(moveCooldownTime);

            ShootProjectile();
        }
    }

    private void DetermineNewLane(float countdownTime, int randomNum)
    {
        if (timeUntilCanMove <= 0 && isMoving == false)
        {
            isMoving = true;

            newBossLaneNumber = randomNum;

            if (newBossLaneNumber != bossLaneNumber)
            {
                positionToLerpTo = new Vector3(transform.position.x + ((bossLaneNumber - newBossLaneNumber) * -1 * 4.275f), transform.position.y, transform.position.z);
            }
            else if (newBossLaneNumber == bossLaneNumber)
            {
                bossLaneNumber = newBossLaneNumber;
                timeUntilCanMove = countdownTime;
                isMoving = false;
            }
        }
    }

    private void SmoothMove(float countdownTime)
    {
        if (isMoving == true)
        {
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, positionToLerpTo, moveSpeed * Time.deltaTime);

            transform.position = smoothedPosition;

            if (transform.position == positionToLerpTo)
            {
                bossLaneNumber = newBossLaneNumber;
                timeUntilCanMove = countdownTime;
                isMoving = false;
            }
        }
    }

    private void ShootProjectile()
    {
        if (timeUntilShoot <= 0 && isMoving == false)
        {
            timeUntilShoot = shootCooldownTime;

            GameObject projectile;

            if (UnityEngine.Random.Range(0, 11) <= 7)
            {
                projectile = Instantiate(projectilePrefab, new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z) + transform.forward * -1.2f, transform.rotation);
            }
            else
            {
                projectile = Instantiate(horizontalProjectilePrefab, new Vector3(transform.position.x, 0.5f, transform.position.z) + transform.forward * -1.2f, transform.rotation);
            }
        }
    }

    private int DealDamage()
    {
        return 10;
    }

    private void TakeDamage(int damage)   //take damage from player typing
    {
        bossHealth -= damage;
        bossHealthText.text = bossHealth + "%";

        if (bossHealth <= 0)
        {
            InterfaceController.gameIsPaused = true;
            InterfaceController.ReloadCurrentScene?.Invoke();
        }
    }

    private Vector3 ShareLocation()
    {
        return transform.position;
    }

    private void ProcessMonitorText()
    {
        if (timeUntilCanMove == moveCooldownTime)
        {
            StartCoroutine(MonitorTextEllipsesTyping());
        }
        else if (timeUntilCanMove <= 1.5f && isMoving == false && monitorIsTyping == false)
        {
            monitorScreenText.text = randomNumber.ToString();

            bossHealthPercentage.SetActive(false);
            lennyFace.SetActive(true);
        }
        else if (isMoving == true)
        {
            if (monitorIsTyping == false)
            {
                StartCoroutine(MonitorTextEllipsesTyping());
            }
        }
    }

    private IEnumerator MonitorTextEllipsesTyping()
    {
        monitorIsTyping = true;

        lennyFace.SetActive(false);
        bossHealthPercentage.SetActive(true);

        monitorScreenText.text = "";

        yield return new WaitForSeconds(0.133f);

        monitorScreenText.text = ".";

        yield return new WaitForSeconds(0.133f);

        monitorScreenText.text = "..";

        yield return new WaitForSeconds(0.133f);

        monitorScreenText.text = "...";

        yield return new WaitForSeconds(0.133f);

        monitorIsTyping = false;
    }
}
