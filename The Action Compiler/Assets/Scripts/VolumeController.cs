using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumeController : MonoBehaviour
{
    [SerializeField] Volume volume;

    public static Action ApplyDamageEffects;
    public static Action RemoveDamageEffects;

    private ChromaticAberration chromaticAberration;
    private ColorAdjustments colorAdjustments;

    private float timeUntilRestoreScreen = 15f;
    private bool shouldRestoreScreen = false;

    private void OnEnable()
    {
        ApplyDamageEffects += PlayerTakeDamageEffects;
    }

    private void OnDisable()
    {
        ApplyDamageEffects -= PlayerTakeDamageEffects;
    }


    private void Update()
    {
        if (!InterfaceController.gameIsPaused)
        {
            if (shouldRestoreScreen == true)
            {
                timeUntilRestoreScreen -= Time.deltaTime;
            }
            if (timeUntilRestoreScreen <= 0)
            {
                StartCoroutine(PlayerRemoveDamageEffects());
            }
        }
    }


    private void PlayerTakeDamageEffects()
    {
        if (volume.profile.TryGet<ChromaticAberration>(out chromaticAberration))
        {
            chromaticAberration.intensity.value += 0.1f;
        }

        if (volume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            colorAdjustments.saturation.value -= 10f;
        }

        shouldRestoreScreen = true;
        timeUntilRestoreScreen = 15f;
    }

    private IEnumerator PlayerRemoveDamageEffects()
    {
        if (volume.profile.TryGet<ChromaticAberration>(out chromaticAberration))
        {
            if (chromaticAberration.intensity.value > 0)
            {
                chromaticAberration.intensity.value -= 0.05f;
            }
        }

        if (volume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            if (colorAdjustments.saturation.value < 0)
            {
                colorAdjustments.saturation.value += 5f;
            }
            if (colorAdjustments.saturation.value > 0)
            {
                colorAdjustments.saturation.value = 0;
            }
        }

        if (chromaticAberration.intensity.value == 0 && colorAdjustments.saturation == 0)
        {
            shouldRestoreScreen = false;
            timeUntilRestoreScreen = 15f;

            Player.RestoreHealthToFull?.Invoke();   // after the screen is restored back to normal, the player's health is restored too
        }

        yield return new WaitForSeconds(0.2f);
    }
}
