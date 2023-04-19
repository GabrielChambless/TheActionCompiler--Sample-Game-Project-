using System.Collections.Generic;
using UnityEngine;

public class BackgroundObjectsController : MonoBehaviour
{
    [SerializeField] private List<GameObject> allBackgroundObjects = new List<GameObject>();

    private List<Quaternion> allRandomRotations = new List<Quaternion>();

    private float rotateSpeed = 0.25f;

    private void Start()
    {
        for (int i = 0; i < allBackgroundObjects.Count; i++)
        {
            allRandomRotations.Add(Quaternion.Euler(Random.Range(-360f, 360f), Random.Range(-360f, 360f), Random.Range(-360f, 360f)));
            allBackgroundObjects[i].transform.rotation = allRandomRotations[i];
        }
    }

    private void Update()
    {
        for (int i = 0; i < allBackgroundObjects.Count; i++)
        {
            allBackgroundObjects[i].transform.Rotate(allRandomRotations[i].eulerAngles * rotateSpeed * Time.deltaTime);
        }
    }
}
