using System;
using UnityEngine;

public class NormalDistribution
{
    private System.Random random;
    private float mean;
    private float stdDev;

    public NormalDistribution(float mean, float stdDev)
    {
        if (stdDev <= 0)
        {
            throw new ArgumentException("Standard deviation must be greater than zero.");
        }

        this.mean = mean;
        this.stdDev = stdDev;
        this.random = new System.Random();
    }

    public float NextFloat()
    {
        double u1 = 1.0 - random.NextDouble(); // Uniform(0,1] random doubles
        double u2 = 1.0 - random.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                               Math.Sin(2.0 * Math.PI * u2); // Box-Muller transform for standard normal distribution
        float randNormal = (float)(mean + stdDev * randStdNormal);
        return randNormal;
    }

    // Override the implicit conversion operator to convert NormalDistribution to float
    public static implicit operator float(NormalDistribution nd)
    {
        return nd.NextFloat();
    }
}