using UnityEngine;
using System.Collections;
using Graph;

public class PCGnode {
	// Static variables
	// TODO: Could maybe replace these with a #define, but I'm not sure it'd make much difference
	private static int minWidth = 2;
	private static int minHeight = 2;
	public static int MinWidth { get { return PCGnode.minWidth; } }
	public static int MinHeight { get { return PCGnode.minHeight; } }

	PCGnode leftChild, rightChild;
	private int myWidth, myHeight;
	private IntVec2 myTopLeft;

	public PCGnode Left { get { return leftChild; } }
	public PCGnode Right { get { return rightChild; } }
	public IntVec2 TopLeft { get { return myTopLeft; } }
	public int Width { get { return myWidth; } }
	public int Height { get { return myHeight; } }

	static public PCGnode getOrigin (int width, int height) {
		PCGnode temp = new PCGnode();
		temp.myTopLeft = new IntVec2(0, 0);
		temp.myWidth = width;
		temp.myHeight = height;

		return temp;
	}

	public PCGnode() { leftChild = rightChild = null; }

	public bool split(PCG.SplitDirection dir, float ratio) {
		// Check that the children haven't already been set
		if ((leftChild == null) && (rightChild == null)) {
			// Horizontal split -> a horizontal line that separates the area in two
			if (dir == PCG.SplitDirection.Horizontal) {
				int leftHeight = (int)(ratio * myHeight);
				int rightHeight = myHeight - leftHeight;

				// Check that neither child will end up too small
				if(leftHeight >= PCGnode.minHeight && 
				  (rightHeight >= PCGnode.minHeight)) {
					leftChild = new PCGnode();
					leftChild.myTopLeft = new IntVec2(myTopLeft.x, myTopLeft.y);
					leftChild.myWidth = myWidth;
					leftChild.myHeight = leftHeight;

					rightChild = new PCGnode();
					rightChild.myTopLeft = myTopLeft + (new IntVec2(0, leftHeight));
					rightChild.myWidth = myWidth;
					rightChild.myHeight = rightHeight;

					return true;
				}
			} else {
				// Else: Split is vertical
				int leftWidth = (int)(ratio * myWidth);
				int rightWidth = myWidth - leftWidth;

				// Check that neither child will end up too small
				if ((leftWidth >= PCGnode.minWidth) &&
				   (rightWidth >= PCGnode.minWidth)) {
					leftChild = new PCGnode();
					leftChild.myTopLeft = new IntVec2(myTopLeft.x, myTopLeft.y);
					leftChild.myWidth = leftWidth;
					leftChild.myHeight = myHeight;

					rightChild = new PCGnode();
					rightChild.myTopLeft = myTopLeft + (new IntVec2(leftWidth, 0));
					rightChild.myWidth = rightWidth;
					rightChild.myHeight = myHeight;

					return true;
				   }
			}
		}

		// Either the child nodes already exist, or the calculated child nodes would end up too small
		return false;
	}

	override public string ToString() {
		return "Node: " + "(" + myTopLeft.x + ", " + myTopLeft.y + "), w = " + myWidth + ", h = " + myHeight;
	}
}
