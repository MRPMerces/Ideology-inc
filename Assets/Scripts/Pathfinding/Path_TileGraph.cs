using System.Collections.Generic;
using UnityEngine;

public class Path_TileGraph {

    // This class constructs a simple path-finding compatible graph
    // of our world.  Each tile is a node. Each neighbour
    // from a tile is linked via an edge connection.

    public Dictionary<Tile, Path_Node<Tile>> nodes;

    public Path_TileGraph() {

        // Loop through all tiles of the world
        // For each tile, create a node

        nodes = new Dictionary<Tile, Path_Node<Tile>>();

        for (int x = 0; x < World.world.Width; x++)
            for (int y = 0; y < World.world.Height; y++) {

                Tile t = World.world.getTileAt(x, y);

                Path_Node<Tile> n = new Path_Node<Tile> {
                    data = t
                };
                nodes.Add(t, n);
            }

        Debug.Log("Path_TileGraph: Created " + nodes.Count + " nodes.");


        // Now loop through all nodes again
        // Create edges for neighbours

        int edgeCount = 0;

        foreach (Tile t in nodes.Keys) {
            Path_Node<Tile> n = nodes[t];

            List<Path_Edge<Tile>> edges = new List<Path_Edge<Tile>>();

            // Get a list of neighbours for the tile
            Tile[] neighbours = t.getNeighbours(true);  // NOTE: Some of the array spots could be null.

            // Create an edge to the relevant node.
            for (int i = 0; i < neighbours.Length; i++) {
                if (neighbours[i] != null) {
                    // This neighbour exists so create an edge.

                    Path_Edge<Tile> e = new Path_Edge<Tile> {
                        cost = 1, /// Is cost 1? diag
                        node = nodes[neighbours[i]]
                    };

                    // Add the edge to our temporary (and growable!) list
                    edges.Add(e);

                    edgeCount++;
                }
            }

            n.edges = edges.ToArray();
        }
        Debug.Log("Path_TileGraph: Created " + edgeCount + " edges.");
    }
}
