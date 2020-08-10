using System.Collections;
using UnityEngine;
using TMPro;

public class GyroscopeCamera : MonoBehaviour
{
    // STATE
    private float initialPitch = 0f;
    private float appliedGyroPitch = 0f;
    private float calibrationPitch = 0f;
    private float initialYaw = 0f;
    private float appliedGyroYaw = 0f;
    private float calibrationYaw = 0f;
    private Transform rawGyroRotation;
    private float tempSmoothing;

    private Quaternion currentOffset = Quaternion.identity;

    // SETTINGS
    [SerializeField] private float maxPitch = 0f; 
    [SerializeField] private float smoothing = 0.1f;
    [SerializeField] private GameObject resetViewPanel;
    [SerializeField] private TMP_Text debugText;
    [SerializeField] private TMP_Text debugText2;

    Quaternion offset;
    private IEnumerator Start()
    {
        Input.gyro.enabled = true;
        Application.targetFrameRate = 60;
        initialYaw = transform.eulerAngles.y;

        rawGyroRotation = new GameObject("GyroRaw").transform;
        rawGyroRotation.position = transform.position;
        rawGyroRotation.rotation = transform.rotation;

        // Calibrate to reset starting rotation when gyro is active.
        yield return new WaitForSeconds(2);

        StartCoroutine(CalibrateYawAndPitch());
    }

    private void Update()
    {
        ApplyGyroRotation();
        ApplyCalibration();

        transform.rotation =  Quaternion.Slerp(transform.rotation, rawGyroRotation.rotation, smoothing);
        var clampedPitch = Utility.ClampAngle(transform.rotation.eulerAngles.x, -maxPitch, maxPitch); 
        var clampedYaw = Utility.ClampAngle(transform.rotation.eulerAngles.y, -180, 180); 
        transform.rotation = Quaternion.Euler(
            clampedPitch, transform.rotation.eulerAngles.y, 0);


        //debugText.SetText("Rotation: " + Utility.ClampAngle(transform.rotation.eulerAngles.x, -maxPitch, maxPitch));
    }

    private IEnumerator CalibrateYawAndPitch()
    {
        tempSmoothing = smoothing;
        smoothing = 1;
        calibrationPitch = appliedGyroPitch - initialPitch; // Offsets pitch in case it wasn't 0 at edit time.
        calibrationYaw = appliedGyroYaw - initialYaw; // Offsets yaw in case it wasn't 0 at edit time.
        yield return null;
        smoothing = tempSmoothing;
    }

    private void ApplyGyroRotation()
    {
        rawGyroRotation.rotation = Input.gyro.attitude;
        rawGyroRotation.Rotate(0f, 0f, 180f, Space.Self); // Swap "handedness" of quaternion from gyro.
        rawGyroRotation.Rotate(90f, 180f, 0f, Space.World); // Rotate to make sense as a camera pointing out the back of your device.

        appliedGyroPitch = rawGyroRotation.eulerAngles.x; // Save pitch for use in calibration.
        appliedGyroYaw = rawGyroRotation.eulerAngles.y; // Save yaw for use in calibration.
    }

    private void ApplyCalibration()
    {
        offset = transform.rotation * Quaternion.Inverse(GyroToUnity(Input.gyro.attitude));
        // var yawRot = Quaternion.AngleAxis(-calibrationYaw, Vector3.up);
        // rawGyroRotation.rotation *= yawRot;
        // var pitchRot = Quaternion.AngleAxis(-calibrationPitch, Vector3.right);
        // rawGyroRotation.rotation *= pitchRot;
        
        
        //StartCoroutine(Calibrate());

    }

    private static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }

    IEnumerator Calibrate()
    {
        calibrationPitch = Utility.ClampAngle(calibrationPitch, -maxPitch, maxPitch);
        rawGyroRotation.Rotate(calibrationPitch, -calibrationYaw, 0f, Space.World); // Rotates y angle back however much it deviated when calibrationYaw was saved.
        debugText.SetText("yaw: " + calibrationYaw);
        debugText2.SetText("pitch: " + calibrationPitch);
        yield return null;
        //rawGyroRotation.Rotate(0, -calibrationYaw, 0f, Space.World); // Rotates y angle back however much it deviated when calibrationYaw was saved.

    }

    // "Recalibrate" button on click method
    public void ResetGyroscope()
    {
        if (resetViewPanel.activeSelf) {
            resetViewPanel.SetActive(false);
            StartCoroutine(CalibrateYawAndPitch());
        }
        else {
            resetViewPanel.SetActive(true);
        }
    }

    private void OnApplicationFocus(bool focusStatus)
    {
        if (focusStatus)
        {
            StartCoroutine(CalibrateYawAndPitch());
            debugText.SetText("focused");
        }
        else
        {
            StartCoroutine(CalibrateYawAndPitch());
            debugText.SetText("not focused");
        }
    }

    public void OnApplicationPause(bool paused) {
        if(paused) {
            debugText.SetText("pause");
        } else {
            debugText.SetText("not paused");
        }
    }
}
