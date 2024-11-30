using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    public Transform target;

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(target.position, Vector3.up, 50 * Time.deltaTime);
    }
}
