using UnityEngine;

public class BattleFieldBoundary : MonoBehaviour
{
    public static float MinX { get; private set; }
    public static float MaxX { get; private set; }
    public static float MinY { get; private set; }
    public static float MaxY { get; private set; }

    void Awake()
    {
        // ТОЧНЫЕ ГРАНИЦЫ ИЗ РАЗМЕРОВ ПОЛЯ
        MinX = -242f;
        MaxX = 242f;
        MinY = -252f;
        MaxY = 32f;

        Debug.Log($"Границы поля: X[{MinX}..{MaxX}], Y[{MinY}..{MaxY}]");
    }
}