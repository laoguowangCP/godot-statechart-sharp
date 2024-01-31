using System;
using Godot;

namespace LGWCP.Util
{

public class MathUtil
{
    public static float SmoothDamp(
        float current,
        float target,
        ref float velocity,
        float smoothTime,
        float maxSpeed,
        float deltaTime
    )
    {
        float omega = 2.0f / smoothTime;
        float x = omega * deltaTime;
        float exp = 1.0f / (
            1.0f
            + x
            + 0.48f * x * x
            + 0.235f * x * x * x);
        
        float change = current - target;
        float maxChange = maxSpeed * smoothTime;
        change = Math.Clamp(change, -maxChange, maxChange);

        float temp = (velocity + omega * change) * deltaTime;
        velocity = (velocity - omega * temp) * exp;
        float result = target + (change + temp) * exp;

        // Handle overshoot
        if ((target > current) == (result > target))
        {
            result = target;
            velocity = 0.0f;
        }

        return result;
    }
}

} // End of namespace
