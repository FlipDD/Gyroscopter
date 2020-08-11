using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroscopeCamera : MonoBehaviour
{
    [SerializeField] GameObject resetViewPanel;
    [SerializeField] float smoothing = 0.1f;
    [SerializeField] float maxPitch = 40;
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
        transform.localRotation = Quaternion.Euler(Utility.ClampAngle(transform.rotation.eulerAngles.x, -maxPitch, maxPitch), transform.rotation.eulerAngles.y, 0);
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
            StartCoroutine(WaitToCalibrate());
    }

    IEnumerator WaitToCalibrate()
    {
        yield return new WaitForSeconds(.5f);
        Calibrate();
    }
}
