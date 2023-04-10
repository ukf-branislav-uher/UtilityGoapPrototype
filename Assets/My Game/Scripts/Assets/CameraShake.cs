using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour {

	public float shakeMagnetude = 0.05f, shakeTime = 0.5f;
	public Camera mainCamera;

	private float cameraInitialFOV;
	public void ShakeIt()
	{
		cameraInitialFOV = mainCamera.fieldOfView;
		InvokeRepeating ("StartCameraShaking", 0f, 0.005f);
		Invoke ("StopCameraShaking", shakeTime);
	}

	void StartCameraShaking()
	{
		mainCamera.fieldOfView = cameraInitialFOV + Random.value * shakeMagnetude;
	}

	void StopCameraShaking()
	{
		CancelInvoke ("StartCameraShaking");
		mainCamera.fieldOfView = cameraInitialFOV;
	}

}
