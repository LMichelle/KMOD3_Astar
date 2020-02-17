using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary> Notes
/// Don't forget to remove gizmos and monobehaviour functionality from astar class
/// Why can't I return a value only in the while loop?
/// </summary>


public class Astar : MonoBehaviour
{
    public Node[,] nodeGrid;
    List<Vector2Int> finalPath;


    /// <summary>
    /// TODO: Implement this function so that it returns a list of Vector2Int positions which describes a path
    /// from the startPos to the endPos.
    /// Note that you will probably need to add some helper functions
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="grid"></param>
    /// <returns></returns>
    public List<Vector2Int> FindPathToTarget(Vector2Int startPos, Vector2Int endPos, Cell[,] grid)
    {
        if (nodeGrid == null) {
            CreateNodeGrid(grid);
        }
        Node startNode = nodeGrid[startPos.x, startPos.y];  // get current Node from startPos
        Node targetNode = nodeGrid[endPos.x, endPos.y]; // get current Node from endPos

        // ClosedList contains Cells that have been checked, OpenList ones that will be checked.
        List<Node> OpenList = new List<Node>();
        List<Node> ClosedList = new List<Node>();

        OpenList.Add(startNode);

        while (OpenList.Count > 0) {
            Node currentNode = OpenList[0];
            for (int i = 1; i < OpenList.Count; i++) { // check all the nodes in openlist that are not the current
                if (OpenList[i].FCost < currentNode.FCost || OpenList[i].FCost == currentNode.FCost && OpenList[i].HCost < currentNode.HCost) {
                    currentNode = OpenList[i];
                }
            }
            OpenList.Remove(currentNode);
            ClosedList.Add(currentNode);

            if (currentNode == targetNode) {    // if reached the end
                finalPath = GetFinalPath(startNode, targetNode);
                return finalPath;
            }

            // Get the neighbours
            foreach (Node neigbourNode in GetNeighbouringNodes(currentNode)) {
                Debug.Log(GetNeighbouringNodes(currentNode).Count);
                if (ClosedList.Contains(neigbourNode)) { //!neigbourNode.walkable || 
                    continue;
                }
                int moveCost = currentNode.GCost + GetManhattenDistance(currentNode, neigbourNode);
                // check for walls between this node and the neighbournode. 
                // if there, add large cost to movecost (will be the gcost)
                moveCost += GetWallCost(currentNode, neigbourNode, grid);

                if (moveCost < neigbourNode.GCost || !OpenList.Contains(neigbourNode)) {
                    neigbourNode.GCost = moveCost;
                    neigbourNode.HCost = GetManhattenDistance(neigbourNode, targetNode);
                    neigbourNode.parent = currentNode;

                    if (!OpenList.Contains(neigbourNode)) {
                        OpenList.Add(neigbourNode);
                    }
                }
            }
        }
        return finalPath;
    }

    public List<Node> GetNeighbouringNodes(Node n)
    {
        Debug.Log(n.position);
        List<Node> NeighbouringNodesList = new List<Node>();

        // non-diagonally
        for (int xx = -1; xx <= 1; xx++) {
            for (int yy = -1; yy <= 1; yy++) {
                int x = n.position.x + xx;
                int y = n.position.y + yy;
                if (x < 0 || x >= nodeGrid.GetLength(0) || y < 0 || y >= nodeGrid.GetLength(1)) { continue; }
                if (Mathf.Abs(xx) == Mathf.Abs(yy)) { continue; }
                NeighbouringNodesList.Add(nodeGrid[x, y]);
            }
        }

        return NeighbouringNodesList;
    }

    public int GetManhattenDistance(Node startNode, Node endNode)
    {
        int ix = Mathf.Abs(startNode.position.x - endNode.position.x);
        int iy = Mathf.Abs(startNode.position.y - endNode.position.y);

        return iy + ix;
    }

    public int GetWallCost(Node startNode, Node neighbourNode, Cell[,] grid)
    {
        Cell startCell = grid[startNode.position.x, startNode.position.y];
        Cell neighbourCell = grid[neighbourNode.position.x, neighbourNode.position.y];

        // if start has wall on left and neighbour on right 
        // start right neighbour left
        // start up neighbour down
        // start down neighbour up
        // --> then return high value - infinity?

        if (neighbourCell.gridPosition.x < startCell.gridPosition.x ) {
            if (startCell.HasWall(Wall.LEFT) && neighbourCell.HasWall(Wall.RIGHT)) {
                return 10000;
            }
        } else if (neighbourCell.gridPosition.x > startCell.gridPosition.x) {
            if (startCell.HasWall(Wall.RIGHT) && neighbourCell.HasWall(Wall.LEFT)) {
                return 10000;
            }
        } else if (neighbourCell.gridPosition.y < startCell.gridPosition.y) {
            if (startCell.HasWall(Wall.DOWN) && neighbourCell.HasWall(Wall.UP)) {
                return 10000;
            }
        } else if (neighbourCell.gridPosition.y > startCell.gridPosition.y) {
            if (startCell.HasWall(Wall.UP) && neighbourCell.HasWall(Wall.DOWN)) {
                return 10000;
            }
        } else {
            Debug.LogError("neighbourcell doesn't align.");
        }

        //if ((startCell.HasWall(Wall.LEFT) && neighbourCell.HasWall(Wall.RIGHT)) ||
        //    (startCell.HasWall(Wall.RIGHT) && neighbourCell.HasWall(Wall.LEFT)) ||
        //    (startCell.HasWall(Wall.UP) && neighbourCell.HasWall(Wall.DOWN)) ||
        //    (startCell.HasWall(Wall.DOWN) && neighbourCell.HasWall(Wall.UP))) { return 10000; }
        return 0;
    }

    public List<Vector2Int> GetFinalPath(Node startNode, Node endNode)
    {
        List<Node> FinalPath = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode) {
            FinalPath.Add(currentNode);
            currentNode = currentNode.parent;
        }

        FinalPath.Reverse();
        List<Vector2Int> FinalPathV2 = new List<Vector2Int>();
        foreach (Node n in FinalPath) {
            FinalPathV2.Add(n.position);
        }

        return FinalPathV2;
    }

    public void CreateNodeGrid(Cell[,] grid)
    {
        nodeGrid = new Node[grid.GetLength(0), grid.GetLength(1)];
        for (int x = 0; x < grid.GetLength(0); x++) {
            for (int y = 0; y < grid.GetLength(1); y++) {
                nodeGrid[x, y] = new Node(grid[x, y].gridPosition, null, 0, 0);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (nodeGrid != null) {
            foreach(Node n in nodeGrid) {
                Gizmos.color = Color.white;
                Gizmos.DrawCube(new Vector3(n.position.x, 0, n.position.y), Vector3.one * .6f);
            }
        }

        if (finalPath != null) {
            foreach (Vector2Int pos in finalPath) {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(new Vector3(pos.x, 0, pos.y), Vector3.one * .5f);
            }
        }
        
    }

    /// <summary>
    /// This is the Node class. You can use this class to store calculated FScores for the cells of the grid, you can leave this as it is.
    /// </summary>
    public class Node
    {
        public Vector2Int position; //Position on the grid
        public Node parent; //Parent Node of this node

        public int FCost { //GScore + HScore
            get { return GCost + HCost; }
        }
        public int GCost; //Current Travelled Distance
        public int HCost; //Distance estimated based on Heuristic

        public Node() { }
        public Node(Vector2Int position, Node parent, int GCost, int HCost)
        {
            this.position = position;
            this.parent = parent;
            this.GCost = GCost;
            this.HCost = HCost;
        }
    }
}
