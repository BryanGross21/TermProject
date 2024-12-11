using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class DayToNight : MonoBehaviour
{
    [SerializeField] private Light sun;
    [SerializeField, Range(0,24)] private float timeOfDay;
    [SerializeField] private float sunRotationSpeed;
    [SerializeField] private bool cycleGoing;
	[SerializeField] private Gradient sunColor;

	private float currentTime;

	private void Update()
    {
        if (cycleGoing)
        {
            if (sunRotationSpeed < 1) 
            {
                sunRotationSpeed = 1;
            }

            timeOfDay += Time.deltaTime * sunRotationSpeed;

            if (timeOfDay > 24)
            {
                timeOfDay = 0;
            }

            UpdateSunRotation();
            UpdateLighting();
        }
    }

    private void OnValidate() 
    {
		UpdateSunRotation();
		UpdateLighting();
	}

    private void UpdateSunRotation() 
    {
        currentTime = timeOfDay / 24;

        float sunRotation = Mathf.Lerp(-90, 270, currentTime);

        sun.transform.rotation = Quaternion.Euler(sunRotation, 0, 0);
    }

    private void UpdateLighting() 
    {
        sun.color = sunColor.Evaluate(currentTime);
    }

}
