using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InterfaceController : MonoBehaviour
{
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private GameObject menuGroup;
    [SerializeField] private GameObject actionInventoryScreen;

    [SerializeField] private GameObject playerCanvas;

    public static Action TogglePlayerCanvas;
    public static Action ToggleMenuCanvas;
    public static Action ReloadCurrentScene;

    public static bool gameIsPaused = true;


    private void OnEnable()
    {
        TogglePlayerCanvas += TogglePlayerUI;
        ToggleMenuCanvas += EnableMenuUI;
        ReloadCurrentScene += ReloadScene;
    }

    private void OnDisable()
    {
        TogglePlayerCanvas -= TogglePlayerUI;
        ToggleMenuCanvas -= EnableMenuUI;
        ReloadCurrentScene -= ReloadScene;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PlayPauseGame();
        }

        MenuMonitorController.MatchMenuMonitor?.Invoke();
    }

    public void PlayPauseGame()
    {
        if (gameIsPaused == false)
        {
            gameIsPaused = true;

            Player.cameraInPlace = false;

            playerCanvas.SetActive(false);
        }
        else
        {
            gameIsPaused = false;

            Player.cameraInPlace = false;

            menuCanvas.SetActive(false);
        }
    }

    public void ToggleActionInventoryScreen()
    {
        if (actionInventoryScreen.activeSelf == false)
        {
            menuGroup.SetActive(false);
            actionInventoryScreen.SetActive(true);
        }
        else
        {
            actionInventoryScreen.SetActive(false);
            menuGroup.SetActive(true);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void TogglePlayerUI()
    {
        if (playerCanvas.activeSelf == false)
        {
            playerCanvas.SetActive(true);
        }
        else
        {
            playerCanvas.SetActive(false);
        }
    }

    private void EnableMenuUI()
    {
        playerCanvas.SetActive(false);
        menuCanvas.SetActive(true);
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
