using UnityEngine;

public class LightShadowDebugger : MonoBehaviour
{
    void Start()
    {
        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);

        int totalMaps = 0;

        Debug.Log("===== SHADOW MAP DEBUG =====");

        foreach (Light l in lights)
        {
            if (!l.gameObject.activeInHierarchy || !l.enabled)
                continue;

            int maps = 0;

            if (l.shadows != LightShadows.None)
            {
                switch (l.type)
                {
                    case LightType.Directional:
                        maps = 1;
                        break;

                    case LightType.Spot:
                        maps = 1;
                        break;

                    case LightType.Point:
                        maps = 6;
                        break;
                }
            }

            totalMaps += maps;

            Debug.Log(
                "Name: " + l.name +
                " | Type: " + l.type +
                " | Shadows: " + l.shadows +
                " | ShadowMaps: " + maps
            );
        }

        Debug.Log("TOTAL SHADOW MAPS: " + totalMaps);
    }
}
