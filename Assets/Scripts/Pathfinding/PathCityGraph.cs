using System.Collections.Generic;
using UnityEngine;

public class PathCityGraph : PathGraph {
    public PathCityGraph() : base() {
        // Generate graph for city based on walkable sidewalks and connections
        for (int x = 0; x < CityManager.Instance.Width(); x++) {
            for (int y = 0; y < CityManager.Instance.Height(); y++) {
                if (CityManager.Instance.IsWalkable(x, y)) {
                    PathNode node = new PathNode();
                    node.data = new Vector2(x, y);
                    nodes.Add(new Vector2(x, y), node);
                }
            }
        }

        foreach (Vector2 pos in nodes.Keys) {
            PathNode node = nodes[pos];
            List<PathEdge> edges = new List<PathEdge>();
            Vector2[] neighbors = CityManager.Instance.BuildingNeighbors((int)pos.x, (int)pos.y, true);
            for (int i = 0; i < neighbors.Length; i++) {
                if (neighbors[i].x != -1 && neighbors[i].y != -1) {
                    PathEdge edge = new PathEdge();
                    if (CityManager.Instance.IsWalkable((int)neighbors[i].x, (int)neighbors[i].y)) {
                        edge.cost = 1;
                    } else {
                        edge.cost = 100;
                    }
                    edge.node = nodes[neighbors[i]];
                    edges.Add(edge);
                }
            }
            node.edges = edges.ToArray();
        }
    }
}