using UnityEngine;

public static class Utility
{
    public static float ClampAngle(float angle, float min, float max) 
    {
        if (angle > 180) angle -= 360;
        if (angle > max) angle = max;
        if (angle < min) angle = min;
        // float start = (min + max) * 0.5f - 180;
        // float floor = Mathf.FloorToInt((angle - start) / 360) * 360;
        // min += floor;
        // max += floor;
        //return Mathf.Clamp(angle, min, max);
        return angle;
    }
}
