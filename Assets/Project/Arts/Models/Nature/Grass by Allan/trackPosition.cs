using UnityEngine;

public class trackPosition : MonoBehaviour
{
    [SerializeField] private GameObject tracker;
    [SerializeField] private Material grassMat;

    void Start()
    {
        if (tracker == null)
            Debug.LogError("Tracker gameObject not set for grass");

        if (grassMat == null)
            Debug.LogError("Grass Material not set for grass");
    }

    void Update()
    {
        Vector3 trackerPos = tracker.transform.position;
        grassMat.SetVector("_trackerPosition", trackerPos);
    }
}
