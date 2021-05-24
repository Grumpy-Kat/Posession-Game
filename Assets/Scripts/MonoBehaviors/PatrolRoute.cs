using System;
using UnityEngine;

public class PatrolRoute : MonoBehaviour {
    [Serializable]
    public class PatrolStop {
        public Transform stopPos;
        public float stopTime;
    }

    // PatrolRoute is used as parent for PatrolStops in order to create markers for route for AI
    public PatrolStop[] patrolStops;
    public int currIndex { get; private set; }

    public void SetCurrIndex(int newIndex) {
        currIndex = newIndex;
        if (currIndex < 0) {
            currIndex = patrolStops.Length - 1;
        }
        if (currIndex >= patrolStops.Length) {
            currIndex = 0;
        }
    }
}

