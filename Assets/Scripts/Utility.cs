using UnityEngine;

public static class Utility
{
    public static float ClampAngle(float angle, float min, float max) 
    {
        if (angle > 180) angle -= 360;
        
        if (angle > max) angle = max;
        else if (angle < min) angle = min;
        return angle;
    }
}
