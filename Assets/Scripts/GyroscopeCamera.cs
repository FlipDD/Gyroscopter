using System.Collections;
using UnityEngine;

public class GyroscopeCamera : MonoBehaviour
{
    [SerializeField] float maxPitch = -40f;
    [SerializeField] float smoothing = 0.1f;
    [SerializeField] GameObject resetViewPanel;

    Transform rawGyroRotation;
    Quaternion newAngle, currentAngle, currentOffset;
    float initialPitch, appliedGyroPitch, calibrationPitch;
    float initialYaw, appliedGyroYaw, calibrationYaw;

    private void Awake() => Input.gyro.enabled = true;

    private IEnumerator Start()
    {
        initialPitch = transform.eulerAngles.x;
        initialYaw = transform.eulerAngles.y;

        rawGyroRotation = new GameObject("GyroRaw").transform;
        rawGyroRotation.position = transform.position;
        rawGyroRotation.rotation = transform.rotation;

        // Wait until gyro is active, then calibrate to reset starting rotation.
        yield return new WaitForSeconds(1);

        StartCoroutine(CalibrateAngle());
    }

    private void Update()
    {
        ApplyGyroRotation();
        StartCoroutine(ApplyCalibration());

        // Stop moving towards center if x and y are within a certain range of center
        newAngle = rawGyroRotation.rotation * Quaternion.Inverse(currentOffset);
        transform.rotation = Quaternion.Slerp(transform.rotation, newAngle, smoothing);

        // Clamp pitch and assign to rotation with roll as 0 (no need for roll rotation)
        var clampedPitch = Utility.ClampAngle(transform.rotation.eulerAngles.x, -maxPitch, maxPitch);
        transform.rotation = Quaternion.Euler(clampedPitch, transform.rotation.eulerAngles.y, 0f);
    }

    public IEnumerator CalibrateAngle()
    {
        // Offsets the angle in case it wasn't 0 at edit time.
        calibrationPitch = appliedGyroPitch - initialPitch;
        calibrationYaw = appliedGyroYaw - initialYaw;
        yield return null;
        currentOffset = rawGyroRotation.rotation;
        currentAngle = transform.rotation;
    }

    private void ApplyGyroRotation()
    {
        rawGyroRotation.rotation =
            new Quaternion(.5f, .5f, -.5f, .5f) * Input.gyro.attitude * new Quaternion(0, 0, 1, 0);

        // Save the angle around axis for use in calibration.
        appliedGyroPitch = rawGyroRotation.eulerAngles.x;
        appliedGyroYaw = rawGyroRotation.eulerAngles.y;
    }

    // Avoid gymbal lock
    private IEnumerator ApplyCalibration()
    {
        rawGyroRotation.Rotate(0, -calibrationYaw, 0, Space.World);
        yield return null;
        rawGyroRotation.Rotate(calibrationPitch, 0, 0, Space.World);
    }

    // "Recalibrate" button on click method
    public void ResetGyroscope()
    {
        if (resetViewPanel.activeSelf) {
            resetViewPanel.SetActive(false);
            StartCoroutine(CalibrateAngle());
        }
        else {
            resetViewPanel.SetActive(true);
        }
    }

    // Recalibrate when the app in reopened
    private void OnApplicationFocus(bool focusStatus)
    {
        if (focusStatus)
            StartCoroutine(CalibrateAngle());
    }
}
