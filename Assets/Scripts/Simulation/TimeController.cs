using CircularBuffer;
using UnityEngine;

public class TimeController : MonoBehaviour {
    public int TargetFPS = 30;

    private const int FPS_BUFFER_SIZE = 10;
    private const float UPDATE_FREQUENCY = 1f / FPS_BUFFER_SIZE;
    private float elapsed = 0f;

    private CircularBuffer<float> fpsBuffer = new(FPS_BUFFER_SIZE);
    
    
    // private const float PROPORTIONAL_GAIN = 1f;
    // private const float INTEGRAL_GAIN = 3f;
    // private const float DERIVATIVE_GAIN = 0f;
    // private PIDController pidController = new(PROPORTIONAL_GAIN, INTEGRAL_GAIN, DERIVATIVE_GAIN, 1f, 100f);

    void Start() {
        elapsed = 0f;
        // pidController.SetPoint = TargetFPS;
    }

    void Update() {
        elapsed += Time.deltaTime;

        float currentFPS = 1f / Time.deltaTime;
        fpsBuffer.PushBack(currentFPS);
        if (elapsed > UPDATE_FREQUENCY) {
            elapsed = 0f;

            float currentFPSAvg = CurrentFPSAverage();
            AdjustTimeScale(currentFPSAvg);
            Debug.Log($"CurrentFPSAVG: {currentFPSAvg}");
        }
    }

    private float CurrentFPSAverage() {
        float avgFPS = 0f;
        foreach (float fpsCount in fpsBuffer) {
            avgFPS += fpsCount;
        }
        avgFPS /= fpsBuffer.Size;
        return avgFPS;
    }
    
    private void AdjustTimeScale(float avgFPS) {
        float scaleFactor = avgFPS / TargetFPS;
        float newTimeScale = Time.timeScale * scaleFactor;
        Time.timeScale = Mathf.Clamp(newTimeScale, 1f, 100f);
        //update FixedDeltaTime?
    }
}
