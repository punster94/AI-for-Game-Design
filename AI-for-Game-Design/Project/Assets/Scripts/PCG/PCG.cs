#define DEBUG_PCG // Debug the PCG

using UnityEngine;
using System.Collections.Generic;
using Graph;

public class PCG {
	// TODO: Might replace these with a #define
	int randomSeed;
	int mapWidth = 39;
	int mapHeight = 24;
	// TODO: Fine-tune frequencies
	// Frequencies of different tile types
	int slipperyFreq = 1;
	int sandFreq = 1;
	int tableFreq = 4;
	// Within the room type, tiles vary a bit so the room isn't entirely uniform
	// Room tiles will have a 1 in "roomInteriorVariance" chance of being
	// something different from its room-define type.
	int roomInteriorVariance = 5;

	int wallSparseness = 3; // A wall will have a 1 in "wallSparseness" chance of spawning
	int numSquareTypes = System.Enum.GetNames(typeof(Node.SquareType)).Length; // readability

	public enum SplitDirection { Horizontal, Vertical };
	PCGnode root;

	public PCG()
	{
		root = PCGnode.getOrigin(mapWidth, mapHeight);
        randomSeed = (int)System.DateTime.Now.Ticks;
        Debug.Log("Seed: " + randomSeed);
        // Good seeds:
        // flat area: -999266259
    }

    private Node.SquareType randomOther(Node.SquareType given)
	{
#if DEBUG_PCG
		int tries = 0;
#endif
		int squareTypeNum = (int)given;
		int totalFreq = sandFreq + slipperyFreq + tableFreq;
		// Extremely inefficient, horrible code
		while(squareTypeNum == (int)given)
		{
			// TODO: Double check for off-by-one error here
			// Documentation of Random.Range lies, at least for ints
			// Second number is INCLUSIVE
			squareTypeNum = Random.Range(0, totalFreq - 1);
			if(squareTypeNum < sandFreq) {
				squareTypeNum = (int)Node.SquareType.Sandpaper;
			}
			else if (squareTypeNum < sandFreq + slipperyFreq) {
				squareTypeNum = (int)Node.SquareType.Slippery;
			} else {
				squareTypeNum = (int)Node.SquareType.TableDef;
			}
#if DEBUG_PCG
			tries++;
#endif
		}
#if DEBUG_PCG
		Debug.Log("randomOther(" + (int)given + ") accepted " + squareTypeNum + " after " + tries + " tries.");
#endif
		return (Node.SquareType)squareTypeNum;
	}

	public Node.SquareType[][] generateMap()
	{
		// generateMap a PCG map in a BFS manner
		Random.seed = randomSeed;

		Queue<PCGnode> nodesToProcess = new Queue<PCGnode>();
		nodesToProcess.Enqueue(root);

		Queue<PCGnode> finalNodes = new Queue<PCGnode>();

		// Loop through until our unprocessed nodes queue is empty
		while(nodesToProcess.Count > 0) {
			PCGnode temp = nodesToProcess.Dequeue();

			int r = Random.Range(0, 2);
			float ratio = Random.value;
#if DEBUG_PCG
			Debug.Log("temp = " + temp.ToString());
			Debug.Log("Generated pair: r = " + r + ", ratio = " + ratio);
#endif

			if(temp.split((PCG.SplitDirection)r, ratio)) {
				// If split returns true, the split succeeded
				// Add the children to the process queue and continue the loop
				nodesToProcess.Enqueue(temp.Left);
				nodesToProcess.Enqueue(temp.Right);
			} else {
				// If split returns false, it means the node could not be split
				// This means one or both child nodes would be too small
				// So we'll add this (unsplit) node to our final nodes
				finalNodes.Enqueue(temp);
			}

		}
		// Array of values to generate the map
		Node.SquareType[][] map = new Node.SquareType[mapHeight][];
		for (int y = 0; y < mapHeight; y++)
		{
			map[y] = new Node.SquareType[mapWidth];
		}

#if DEBUG_PCG
		// DEBUG: Array showing how many times each node was written
		// (To check that our PCGnodes do not overlap)
		int[,] writes = new int[mapWidth, mapHeight];
#endif

		while(finalNodes.Count > 0) {
			// Grab a PCGnode
			PCGnode currentNode = finalNodes.Dequeue();
			// generateMap a floor type from zero to number of tile types defined
			Node.SquareType floorType = Node.randWalkState();
#if DEBUG_PCG
			Debug.Log("Random.Range(0," + (numSquareTypes - 1)+") = "+floorType);
#endif
			// Find the top-left of this PCGnode
			IntVec2 start = currentNode.TopLeft;

			for(int OffsetY = 0; OffsetY < currentNode.Height; OffsetY++ ) {
				for(int OffsetX = 0; OffsetX < currentNode.Width; OffsetX++) {
					int x = start.x + OffsetX;
					int y = start.y + OffsetY;
					Node.SquareType thisTileFloor = floorType;

					// TODO: Ok, so I want to figure out how to do this without branching
					// TODO: Pretty sure it can be done with bitwise ops and arithemetic

					// Guarantee the map will be surrounded by wall/impassable tiles
					if ((x == 0) || (x == mapWidth - 1) || (y == 0) || (y == mapHeight - 1))
					{
#if DEBUG_PCG
						Debug.Log("Found edge of map?");
#endif
						thisTileFloor = Node.SquareType.Unwalkable;
					}

					// Have a chance of generating a wall tile at the edge of the "room"
					else if ((OffsetX == currentNode.Width - 1) || (OffsetY == currentNode.Height - 1))
					{
						if (Random.Range(0, wallSparseness) == 0)
						{
#if DEBUG_PCG
							Debug.Log("Making interal wall");
#endif
							thisTileFloor = Node.SquareType.Unwalkable;
						}
					}
					else if(Random.Range(0, roomInteriorVariance - 1) == 0)
					{
						thisTileFloor = randomOther(thisTileFloor);
					}

					// Otherwise, just plop the floor type
					map[y][x] = thisTileFloor;
					
#if DEBUG_PCG
					writes[x, y]++;
#endif
				}
			}
		}
#if DEBUG_PCG
		// Print map

		string mapString = "";
		for(int y = 0; y < mapHeight; y++) {
			for(int x = 0; x < mapWidth; x++) {
				mapString += ((int)map[y][x]).ToString() + " ";
			}
			mapString += "\n";
		}
		string writesString = "";
		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{
				writesString += writes[x, y].ToString() + " ";
			}
			writesString += "\n";
		}

		Debug.Log("Writes:");
		Debug.Log(writesString);
		Debug.Log("Map generated:");
		Debug.Log(mapString);

#endif
		// Reset Random to system time to ensure sufficient randomness to gameplay
		Random.seed = (int)System.DateTime.Now.Ticks;

		return map;
	}
}
