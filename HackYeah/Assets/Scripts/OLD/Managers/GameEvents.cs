using System;

public static class GameEvents
{
    // Event to change the speed of platforms
    public static event Action<float> OnSpeedMultiplierChanged;
    public static void TriggerSpeedMultiplierChanged(float newMultiplier) => OnSpeedMultiplierChanged?.Invoke(newMultiplier);

    // Event to destroy all platforms
    public static event Action OnDestroyAllPlatforms;
    public static void TriggerDestroyAllPlatforms() => OnDestroyAllPlatforms?.Invoke();

    // Event for gravity inversion
    public static event Action<bool> OnGravityInverted;
    public static void TriggerGravityInverted(bool isReversed) => OnGravityInverted?.Invoke(isReversed);

    // Event for when the game is over
    public static event Action OnGameOver;
    public static void TriggerGameOver() => OnGameOver?.Invoke();
}
