using System;

[Serializable]
public class SavedData
{
    public int coins;
    public float maxDepth;
    public int maxCatch;

    // Default values for a new save game
    public SavedData()
    {
        coins = 0;
        maxDepth = -15f;
        maxCatch = 3;
    }
}