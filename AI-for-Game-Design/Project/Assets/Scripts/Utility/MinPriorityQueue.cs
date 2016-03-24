using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary>
/// An implementation of a minimum heap-based priority queue.
/// </summary>
/// <typeparam name="TValue">The valuetype of the queue</typeparam>
public class MinPriorityQueue<TValue>
{
    private List<PriNode> heap = new List<PriNode>();
    private Dictionary<TValue, int> mapInd = new Dictionary<TValue, int>();
    private int count = 0;

    /// <summary>
    /// Internal struct: Keeps track of value-priority pairs.
    /// </summary>
    private struct PriNode
    {
        private readonly TValue value;
        private readonly double inversePriority;
        
        //Public getter properities.
        public TValue Value { get { return value; } }
        public double InversePriority { get { return inversePriority; } }

        public PriNode(TValue value, double priority)
        {
            this.value = value;
            this.inversePriority = priority;
        }
        
        // Note: These only compare priority differences.
        public static bool operator <(PriNode a, PriNode b)
        {
            return a.InversePriority < b.InversePriority;
        }

        public static bool operator >(PriNode a, PriNode b)
        {
            return a.InversePriority > b.InversePriority;
        }
        
        public static bool operator <=(PriNode a, PriNode b)
        {
            return !(a.InversePriority > b.InversePriority);
        }

        public static bool operator >=(PriNode a, PriNode b)
        {
            return !(a.InversePriority < b.InversePriority);
        }

    }

    /// <summary>
    /// Swaps two PriNodes in the array at positions i & j.
    /// </summary>
    private void swap(int i, int j)
    {
        PriNode temp = heap[i];
        heap[i] = heap[j];
        heap[j] = temp;
        mapInd[heap[i].Value] = i;
        mapInd[heap[j].Value] = j;
    }

    /// <summary>
    /// Percolates nodes down the minHeap.
    /// </summary>
    /// <param name="curNodLoc">the current node location</param>
    private void percolateDown(int curNodLoc)
    {
        int left = curNodLoc * 2 + 1;
        if (left >= Count)
            return;

        //Check left child node.
        int smallest = curNodLoc;
        if (heap[left] <= heap[smallest])
            smallest = left;

        //Check right child node.
        int right = curNodLoc * 2 + 2;
        if (right < Count && heap[right] <= heap[smallest])
            smallest = right;

        //Swap and percolate down, if necessary.
        if (smallest != curNodLoc)
        {
            swap(curNodLoc, smallest);
            percolateDown(smallest);
        }
    }

    /// <summary>
    /// Percolates a node upwards in the minHeap.
    /// </summary>
    /// <param name="curNodLoc">the current node location</param>
    private void percolateUp(int curNodLoc)
    {
        if (curNodLoc == 0)
            return;

        //If the parent has greater inverse-
        // -Priority, swap it out, percolate up.
        int parent = (curNodLoc - 1) / 2;
        if (heap[parent] > heap[curNodLoc])
        {
            swap(curNodLoc, parent);
            percolateUp(parent);
        }
    }

    /// <summary>
    /// Number of elements in the queue.
    /// </summary>
    public int Count
    {
        get
        {
            return count;
        }
    }

    /// <summary>
    /// Clears the queue of elements.
    /// </summary>
    public void Clear()
    {
        heap.Clear();
        mapInd.Clear();
        count = 0;
    }

    /// <summary>
    /// Enqueues an element with inversePriority iv
    /// </summary>
    /// <param name="v">The element.</param>
    /// <param name="iv">The inverse priority (less is better).</param>
    public void Enqueue(TValue v, double iv)
    {
        mapInd.Add(v, count);
        heap.Add(new PriNode(v, iv));
        count++;
        percolateUp(count - 1);
    }

    /// <summary>
    /// Re-queues an element with inversePriority newIV
    /// </summary>
    /// <param name="v">The element.</param>
    /// <param name="newIV">The new inverse priority (less is better).</param>
    public void Update(TValue v, double newIV)
    {
        double curPriority = currentInversePriority(v);
        if (newIV < curPriority)
            percolateUp(mapInd[v]);
        else if (newIV > curPriority)
            percolateDown(mapInd[v]);
    }

    /// <summary>
    /// Gives the current priority of an element.
    /// </summary>
    /// <param name="v">The element of the queue.</param>
    /// <returns>The inverse priority of the element.</returns>
    public double currentInversePriority(TValue v)
    {
        return heap[mapInd[v]].InversePriority;
    }

    /// <summary>
    /// Returns whether the queue contains the element or not.
    /// </summary>
    /// <param name="v">The element to check for.</param>
    /// <returns>True, if the element is in the queue.</returns>
    public bool Contains(TValue v)
    {
        return mapInd.ContainsKey(v);
    }

    ///Not used, but can compare two generic values x and y with this.
    private bool Compare(TValue x, TValue y)
    {
        return EqualityComparer<TValue>.Default.Equals(x, y);
    }

    /// <summary>
    /// Peeks at the first element in the queue.
    /// Does not remove it.
    /// Throws InvalidOperationException if queue is empty.
    /// </summary>
    /// <returns>The value at the front of the queue.</returns>
    public TValue Peek()
    {
        if (Count == 0)
            throw new InvalidOperationException("Queue empty.");
        return heap[0].Value;
    }

    /// <summary>
    /// Dequeues the first element in the queue, removing it.
    /// Throws InvalidOperationException if queue is empty.
    /// </summary>
    /// <returns>The value at the front of the queue.</returns>
    public TValue Dequeue()
    {
        if (Count == 0)
            throw new InvalidOperationException("Queue empty.");

        TValue retVal = heap[0].Value;
        if (Count == 1)
        {
            Clear();
            return retVal;
        }

        //Re-set mappings
        count--;
        mapInd.Remove(retVal);
        heap[0] = heap[count];
        heap.RemoveAt(count);
        mapInd[heap[0].Value] = 0;
        percolateDown(0);

        return retVal;
    }
}
