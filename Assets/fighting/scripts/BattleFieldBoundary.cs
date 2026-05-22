using UnityEngine;

public class BattleFieldBoundary : MonoBehaviour
{
    public static float MinX { get; private set; }
    public static float MaxX { get; private set; }
    public static float MinY { get; private set; }
    public static float MaxY { get; private set; }

    void Awake()
    {
        MinX = -242f;
        MaxX = 242f;
        MinY = -252f;
        MaxY = 32f;
    }
}