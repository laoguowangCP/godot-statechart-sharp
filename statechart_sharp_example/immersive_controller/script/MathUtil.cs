using System;
using Godot;
namespace LGWCP.Util
{

public class MathUtil
{
    /// <summary>
    /// Smooth value, using spring damper.
    /// </summary>
    /// <param name="current"></param>
    /// <param name="currentVel">Change rate of current</param>
    /// <param name="target"></param>
    /// <param name="targetVel">Change rate of target</param>
    /// <param name="deltaTime"></param>
    /// <param name="smoothTime">Timescale of smoothing</param>
    /// <param name="dampRatio"></param>
    /// <returns></returns>
    public static float SmoothDamp(
        ref float current,
        ref float currentVel,
        float target,
        float targetVel,
        float deltaTime,
        float smoothTime,
        float dampRatio
    )
    {
        /*
        float omega = 2.0f / smoothTime;
        float x = omega * deltaTime;
        float exp = InvExp3(x);
        
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
        */
    }

    /// <summary>
    /// Fast approximation of e^(-X) calculation.
    /// </summary>
    public static float InvExp3(float X)
    {
        float X2 = X * X;
        return 1 / (
            1.0f
            + 1.00746054f * X
            + 0.45053901f * X2
            + 0.25724632f * X2 * X);
    }
}

} // End of namespace
