using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class PathAStar {
    private Queue<Vector2> path;

    public PathAStar(Vector2 startPos, Vector2 endPos) {
        // Choose graph
        if (!BuildingInteriorManager.Instance.isInside) {
            if (CityManager.Instance.PathfindingGraph(true) == null) {
                CityManager.Instance.pathfindingGraph = new PathCityGraph();
            }
        } else {
            if (BuildingInteriorManager.Instance.PathfindingGraph(true) == null) {
                BuildingInteriorManager.Instance.pathfindingGraph = new PathInteriorGraph(null);
            }
            startPos = BuildingInteriorManager.Instance.InsidePos(startPos);
            endPos = BuildingInteriorManager.Instance.InsidePos(endPos);
        }

        // Gather all available walkable nodes
        Dictionary<Vector2, PathNode> nodes = CityManager.Instance.PathfindingGraph().nodes;

        // Check to make sure start and end are walkable to avoid unnecessary calculations
        if (!nodes.ContainsKey(startPos)) {
            Debug.Log("startPos " + startPos + " is unwalkable.");
            return;
        }
        if (!nodes.ContainsKey(endPos)) {
            Debug.Log("endPos " + endPos + " is unwalkable.");
            return;
        }

        PathNode start = nodes[startPos];
        PathNode end = nodes[endPos];

        List<PathNode> closedSet = new List<PathNode>();
        PriorityQueue<PathNode> openSet = new PriorityQueue<PathNode>();

        openSet.Enqueue(start, MoveCostEstimate(start, end));

        Dictionary<PathNode, PathNode> from = new Dictionary<PathNode, PathNode>();
        Dictionary<PathNode, float> gScore = new Dictionary<PathNode, float>();
        Dictionary<PathNode, float> fScore = new Dictionary<PathNode, float>();
        foreach (PathNode node in nodes.Values) {
            gScore[node] = Mathf.Infinity;
            fScore[node] = Mathf.Infinity;
        }
        gScore[start] = 0;
        fScore[end] = MoveCostEstimate(start, end);

        while (openSet.Count > 0) {
            PathNode curr = openSet.Dequeue();
            if (curr == end) {
                ReconstructPath(from, curr);
                return;
            }
            closedSet.Add(curr);
            foreach (PathEdge edge in curr.edges) {
                PathNode neighbor = edge.node;
                if (closedSet.Contains(neighbor)) {
                    continue;
                }
                float moveCost = CityManager.Instance.Distance(curr.data, neighbor.data) * edge.cost;
                float tentativeGScore = gScore[curr] + moveCost;
                if (openSet.Contains(neighbor) && tentativeGScore >= gScore[neighbor]) {
                    continue;
                }
                from[neighbor] = curr;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + MoveCostEstimate(neighbor, end);
                if (!openSet.Contains(neighbor)) {
                    openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }
        path = null;
    }

    private float MoveCostEstimate(PathNode start, PathNode end) {
        return (Mathf.Abs(start.data.x - end.data.x)) + (Mathf.Abs(start.data.y - end.data.y));
    }

    private void ReconstructPath(Dictionary<PathNode, PathNode> from, PathNode curr) {
        Queue<Vector2> totalPath = new Queue<Vector2>();
        totalPath.Enqueue(curr.data);
        while (from.ContainsKey(curr)) {
            curr = from[curr];
            totalPath.Enqueue(curr.data);
        }
        path = new Queue<Vector2>(totalPath.Reverse());
    }

    public Vector2 Dequeue() {
        return (BuildingInteriorManager.Instance.isInside ? BuildingInteriorManager.Instance.OutsidePos(path.Dequeue()) : path.Dequeue());
    }

    public int Length() {
        return ((path == null) ? 0 : path.Count);
    }
}