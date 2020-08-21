using UnityEngine;

[System.Serializable]
public class ActionRange{
    public float min;
    public float max;
    private float defaultValue;
    public float getDefaultInRange()
    {
        return 2.0f * (defaultValue - min)/(max - min) - 1.0f;
    }
}
