using System;
using System.Collections;
using UnityEngine;

public class GyroscopeCamera : MonoBehaviour
{
    [SerializeField] GameObject resetViewPanel;
    [SerializeField] float smoothing = 0.1f;
    Quaternion origin = Quaternion.identity;
	

    void Awake() 
    {
        Input.gyro.enabled = true;
        Screen.orientation = ScreenOrientation.Landscape;
    }

	IEnumerator Start() 
    {
        yield return new WaitForSeconds(1);
		Calibrate();
	}
	
	void Update() 
    {
		transform.localRotation = Quaternion.Slerp(transform.rotation, GyroToUnity(Quaternion.Inverse(origin) * Input.gyro.attitude), smoothing);
        transform.rotation = Quaternion.Euler(Utility.ClampAngle(transform.rotation.eulerAngles.x, -40, 40), transform.rotation.eulerAngles.y, 0);
	}

    private void Calibrate()
    {
        origin = Input.gyro.attitude;
    }

    private static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }

    // "Recalibrate" button on click method
    public void ResetGyroscope()
    {
        if (resetViewPanel.activeSelf) {
            resetViewPanel.SetActive(false);
            Calibrate();
        }
        else {
            resetViewPanel.SetActive(true);
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
            Calibrate();
    }
}
