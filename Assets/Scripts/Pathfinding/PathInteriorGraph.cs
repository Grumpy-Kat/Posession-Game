using System.Collections.Generic;
using UnityEngine;

public class PathInteriorGraph : PathGraph {
    public PathInteriorGraph(IBuildingInteriorManager interior) : base() {
        // Generate graph for navigating interior spaces, such as houses, to avoid walls
        if (interior == null) {
            Debug.Log("interior is null");
            return;
        }

        for (int x = 0; x < interior.Width(); x++) {
            for (int y = 0; y < interior.Height(); y++) {
                PathNode node = new PathNode();
                node.data = new Vector2(x, y);
                nodes.Add(new Vector2(x, y), node);
            }
        }

        foreach (Vector2 pos in nodes.Keys) {
            PathNode node = nodes[pos];
            List<PathEdge> edges = new List<PathEdge>();
            Vector2[] neighbors = interior.Neighbors((int)pos.x, (int)pos.y);
            for (int i = 0; i < neighbors.Length; i++) {
                if (neighbors[i].x != -1 && neighbors[i].y != -1) {
                    PathEdge edge = new PathEdge();
                    if (interior.IsWall((int)pos.x, (int)pos.y, (int)neighbors[i].x, (int)neighbors[i].y)) {
                        edge.cost = 0;
                    } else {
                        edge.cost = 1;
                    }
                    edge.node = nodes[neighbors[i]];
                    edges.Add(edge);
                }
            }
            node.edges = edges.ToArray();
        }
    }
}