﻿#define DEBUG_PCG // Debug the PCG

using UnityEngine;
using System.Collections.Generic;
using Graph;

public class PCG {
	// TODO: Might replace these with a #define
	int randomSeed = 42;
	int mapWidth = 39;
	int mapHeight = 24;
	// TODO: Fine-tune frequencies
	// Frequencies of different tile types
	int slipperyFreq = 1;
	int sandFreq = 1;
	int tableFreq = 4;

	int wallSparseness = 3; // A wall will have a 1 in "wallSparseness" chance of spawning

	public enum SplitDirection { Horizontal, Vertical };
	PCGnode root;

	public PCG()
	{
		root = PCGnode.getOrigin(mapWidth, mapHeight);
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
			Node.SquareType floorType = (Node.SquareType)Random.Range((int)0, System.Enum.GetNames(typeof(Node.SquareType)).Length-1);
#if DEBUG_PCG
			Debug.Log("Random.Range(0," + (System.Enum.GetNames(typeof(Node.SquareType)).Length - 1)+") = "+floorType);
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
						if (Random.Range(0, wallSparseness) == 0) {
#if DEBUG_PCG
							Debug.Log("Making interal wall");
#endif
							thisTileFloor = Node.SquareType.Unwalkable;
						}
					}
#if DEBUG_PCG
#endif
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
