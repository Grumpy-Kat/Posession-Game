using System.Collections.Generic;
using UnityEngine;

public class PathGraph {
    public Dictionary<Vector2, PathNode> nodes;

    public PathGraph() {
        nodes = new Dictionary<Vector2, PathNode>();
    }
}