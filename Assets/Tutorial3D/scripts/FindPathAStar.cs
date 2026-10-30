using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using System;

public class PathMarker {

    public MapLocation location;

    public float G;
    public float H;
    public float F;

    public GameObject marker;
    public PathMarker parent;

    public PathMarker(MapLocation l, float g, float h, float f, GameObject marker, PathMarker p) {

        location = l;
        G = g;
        H = h;
        F = f;
        this.marker = marker;
        parent = p;
    }

    public override bool Equals(object obj) {

        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            return false;
        else
            return location.Equals(((PathMarker)obj).location);
    }

    public override int GetHashCode() => location.GetHashCode();
}

public class FindPathAStar : MonoBehaviour {

    public Maze maze;
    public Material closedMaterial;
    public Material openMaterial;

    List<PathMarker> open = new List<PathMarker>();
    List<PathMarker> closed = new List<PathMarker>();

    public GameObject start;
    public GameObject end;
    public GameObject pathP;

    PathMarker goalNode;
    PathMarker startNode;
    PathMarker lastPos;

    bool done = false;

    void RemoveAllMarkers() {

        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");

        foreach (GameObject m in markers) {

            Destroy(m);
        }
    }

    void BeginSearch() {

        done = false;
        RemoveAllMarkers();

        List<MapLocation> locations = new List<MapLocation>();

        for (int z = 1; z < maze.depth - 1; ++z) {
            for (int x = 1; x < maze.width - 1; ++x) {

                if (maze.map[x, z] != 1) {

                    locations.Add(new MapLocation(x, z));
                }
            }
        }

        locations.Shuffle();

        Vector3 startLocation = new Vector3(locations[0].x * maze.scale, 0.0f, locations[0].z * maze.scale);

        float h0 = Vector2.Distance(
            new Vector2(locations[0].x, locations[0].z),
            new Vector2(locations[1].x, locations[1].z)
            );

        startNode = new PathMarker(new MapLocation(locations[0].x, locations[0].z), 0.0f, h0, h0,
            Instantiate(start, startLocation, Quaternion.identity), null);

        Vector3 goalLocation = new Vector3(locations[1].x * maze.scale, 0.0f, locations[1].z * maze.scale);
        goalNode = new PathMarker(new MapLocation(locations[1].x, locations[1].z), 0.0f, 0.0f, 0.0f,
            Instantiate(end, goalLocation, Quaternion.identity), null);

        open.Clear();
        closed.Clear();

        open.Add(startNode);
        lastPos = startNode;
    }

    void Search(PathMarker thisNode) {
        if (thisNode == null || done) return;
        if (thisNode.Equals(goalNode)) { done = true; return; }

        // El nodo actual deja la lista abierta antes de revisar sus vecinos.
        open.Remove(thisNode);
        if (!IsClosed(thisNode.location)) closed.Add(thisNode);
        thisNode.marker.GetComponent<Renderer>().sharedMaterial = closedMaterial;
        foreach (MapLocation dir in maze.directions) {
            MapLocation neighbour = dir + thisNode.location;
            if (neighbour.x < 1 || neighbour.x >= maze.width || neighbour.z < 1 || neighbour.z >= maze.depth) continue;
            if (maze.map[neighbour.x, neighbour.z] == 1 || IsClosed(neighbour)) continue;
            float g = thisNode.G + Vector2.Distance(thisNode.location.ToVector(), neighbour.ToVector());
            float h = Vector2.Distance(neighbour.ToVector(), goalNode.location.ToVector());
            if (UpdateMarker(neighbour, g, h, g+h, thisNode)) continue;
            var block = Instantiate(pathP, new Vector3(neighbour.x*maze.scale, 0, neighbour.z*maze.scale), Quaternion.identity);
            block.GetComponent<Renderer>().sharedMaterial = openMaterial;
            var candidate = new PathMarker(neighbour, g, h, g+h, block, thisNode);
            SetValues(candidate);
            open.Add(candidate);
        }
        if (open.Count == 0) { done = true; lastPos = null; return; }
        open = open.OrderBy(node => node.F).ThenBy(node => node.H).ToList();
        lastPos = open[0];
    }

    void SetValues(PathMarker node) {
        TextMesh[] values = node.marker.GetComponentsInChildren<TextMesh>();
        values[0].text = "G: " + node.G.ToString("0.00");
        values[1].text = "H: " + node.H.ToString("0.00");
        values[2].text = "F: " + node.F.ToString("0.00");
    }

    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker parent) {
        foreach (PathMarker node in open) {
            if (!node.location.Equals(pos)) continue;
            // Solo se cambia el padre cuando el nuevo recorrido cuesta menos.
            if (g < node.G) {
                node.G=g; node.H=h; node.F=f; node.parent=parent;
                SetValues(node);
            }
            return true;
        }
        return false;
    }

    bool IsClosed(MapLocation marker) {

        foreach (PathMarker p in closed) {

            if (p.location.Equals(marker)) return true;
        }
        return false;
    }

    void Start() {

    }

    private void GetPath() {

        if (!done || lastPos == null || !lastPos.Equals(goalNode)) return;
        RemoveAllMarkers();
        PathMarker begin = lastPos;

        while (begin != null && !startNode.Equals(begin)) { 
        
            Instantiate(pathP, new Vector3(begin.location.x * maze.scale, 0.0f, begin.location.z * maze.scale), 
                Quaternion.identity);

            begin = begin.parent;
        }

        Instantiate(pathP, new Vector3(begin.location.x * maze.scale, 0.0f, begin.location.z * maze.scale),
            Quaternion.identity);
    }

    void Update() {

        if (Keyboard.current == null) return;
        if (Keyboard.current.pKey.wasPressedThisFrame) BeginSearch();
        if (Keyboard.current.cKey.wasPressedThisFrame && !done) Search(lastPos);
        if (Keyboard.current.mKey.wasPressedThisFrame) GetPath();
    }

}