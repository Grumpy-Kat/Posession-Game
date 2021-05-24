using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Priority_Queue {
    public sealed class FastPriorityQueue<T> : IPriorityQueue<T> where T : FastPriorityQueueNode {
        private int nodesNum;
        private T[] nodes;
        private long nodesNumEnqueued;

        public FastPriorityQueue(int maxNodes) {
#if DEBUG
            if (maxNodes <= 0) {
                throw new InvalidOperationException("New queue size cannot be smaller than 1");
            }
#endif
            nodesNum = 0;
            nodes = new T[maxNodes + 1];
            nodesNumEnqueued = 0;
        }

        public int Count {
            get {
                return nodesNum;
            }
        }

        public int MaxSize {
            get {
                return nodes.Length - 1;
            }
        }

#if NET_VERSION_4_5
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Clear() {
            Array.Clear(nodes, 1, nodesNum);
            nodesNum = 0;
        }

#if NET_VERSION_4_5
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Contains(T node) {
#if DEBUG
            if (node == null) {
                throw new ArgumentNullException("node");
            }
            if (node.QueueIndex < 0 || node.QueueIndex >= nodes.Length) {
                throw new InvalidOperationException("node.QueueIndex has been corrupted. Did you change it manually? Or add this node to another queue?");
            }
#endif
            return (nodes[node.QueueIndex] == node);
        }

#if NET_VERSION_4_5
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Enqueue(T node, double priority) {
#if DEBUG
            if (node == null) {
                throw new ArgumentNullException("node");
            }
            if (nodesNum >= nodes.Length - 1) {
                throw new InvalidOperationException("Queue is full - node cannot be added: " + node);
            }
            if (Contains(node)) {
                throw new InvalidOperationException("Node is already enqueued: " + node);
            }
#endif
            node.Priority = priority;
            nodesNum++;
            nodes[nodesNum] = node;
            node.QueueIndex = nodesNum;
            node.InsertionIndex = nodesNumEnqueued++;
            CascadeUp(nodes[nodesNum]);
        }

#if NET_VERSION_4_5
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void Swap(T node1, T node2) {
            nodes[node1.QueueIndex] = node2;
            nodes[node2.QueueIndex] = node1;
            int temp = node1.QueueIndex;
            node1.QueueIndex = node2.QueueIndex;
            node2.QueueIndex = temp;
        }

        private void CascadeUp(T node) {
            int parent = node.QueueIndex / 2;
            while (parent >= 1) {
                T parentNode = nodes[parent];
                if (HasHigherPriority(parentNode, node)) {
                    break;
                }
                Swap(node, parentNode);
                parent = node.QueueIndex / 2;
            }
        }

#if NET_VERSION_4_5
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void CascadeDown(T node) {
            T newParent;
            int finalQueueIndex = node.QueueIndex;
            while (true) {
                newParent = node;
                int childLeftIndex = 2 * finalQueueIndex;
                if (childLeftIndex > nodesNum) {
                    node.QueueIndex = finalQueueIndex;
                    nodes[finalQueueIndex] = node;
                    break;
                }
                T childLeft = nodes[childLeftIndex];
                if (HasHigherPriority(childLeft, newParent)) {
                    newParent = childLeft;
                }
                int childRightIndex = childLeftIndex + 1;
                if (childRightIndex <= nodesNum) {
                    T childRight = nodes[childRightIndex];
                    if (HasHigherPriority(childRight, newParent)) {
                        newParent = childRight;
                    }
                }
                if (newParent != node) {
                    nodes[finalQueueIndex] = newParent;
                    int temp = newParent.QueueIndex;
                    newParent.QueueIndex = finalQueueIndex;
                    finalQueueIndex = temp;
                } else {
                    node.QueueIndex = finalQueueIndex;
                    nodes[finalQueueIndex] = node;
                    break;
                }
            }
        }

#if NET_VERSION_4_5
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool HasHigherPriority(T higher, T lower) {
            return (higher.Priority < lower.Priority || (higher.Priority == lower.Priority && higher.InsertionIndex < lower.InsertionIndex));
        }

        public T Dequeue() {
#if DEBUG
            if (nodesNum <= 0) {
                throw new InvalidOperationException("Cannot call Dequeue() on an empty queue");
            }

            if (!IsValidQueue()) {
                throw new InvalidOperationException("Queue has been corrupted (Did you update a node priority manually instead of calling UpdatePriority()? Or add the same node to two different queues?)");
            }
#endif
            T returnMe = nodes[1];
            Remove(returnMe);
            return returnMe;
        }

        public void Resize(int maxNodes) {
#if DEBUG
            if (maxNodes <= 0) {
                throw new InvalidOperationException("Queue size cannot be smaller than 1");
            }

            if (maxNodes < nodesNum) {
                throw new InvalidOperationException("Called Resize(" + maxNodes + "), but current queue contains " + nodesNum + " nodes");
            }
#endif
            T[] newArray = new T[maxNodes + 1];
            int highestIndexToCopy = Math.Min(maxNodes, nodesNum);
            for (int i = 1; i <= highestIndexToCopy; i++) {
                newArray[i] = nodes[i];
            }
            nodes = newArray;
        }

        public T First {
            get {
#if DEBUG
                if (nodesNum <= 0) {
                    throw new InvalidOperationException("Cannot call .First on an empty queue");
                }
#endif
                return nodes[1];
            }
        }

#if NET_VERSION_4_5
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void UpdatePriority(T node, double priority) {
#if DEBUG
            if (node == null) {
                throw new ArgumentNullException("node");
            }
            if (!Contains(node)) {
                throw new InvalidOperationException("Cannot call UpdatePriority() on a node which is not enqueued: " + node);
            }
#endif
            node.Priority = priority;
            OnNodeUpdated(node);
        }

        private void OnNodeUpdated(T node) {
            int parentIndex = node.QueueIndex / 2;
            T parentNode = nodes[parentIndex];
            if (parentIndex > 0 && HasHigherPriority(node, parentNode)) {
                CascadeUp(node);
            } else {
                CascadeDown(node);
            }
        }

        public void Remove(T node) {
#if DEBUG
            if (node == null) {
                throw new ArgumentNullException("node");
            }
            if (!Contains(node)) {
                throw new InvalidOperationException("Cannot call Remove() on a node which is not enqueued: " + node);
            }
#endif
            if (node.QueueIndex == nodesNum) {
                nodes[nodesNum] = null;
                nodesNum--;
                return;
            }
            T formerLastNode = nodes[nodesNum];
            Swap(node, formerLastNode);
            nodes[nodesNum] = null;
            nodesNum--;
            OnNodeUpdated(formerLastNode);
        }

        public IEnumerator<T> GetEnumerator() {
            for (int i = 1; i <= nodesNum; i++) {
                yield return nodes[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public bool IsValidQueue() {
            for (int i = 1; i < nodes.Length; i++) {
                if (nodes[i] != null) {
                    int childLeftIndex = 2 * i;
                    if (childLeftIndex < nodes.Length && nodes[childLeftIndex] != null && HasHigherPriority(nodes[childLeftIndex], nodes[i])) {
                        return false;
                    }
                    int childRightIndex = childLeftIndex + 1;
                    if (childRightIndex < nodes.Length && nodes[childRightIndex] != null && HasHigherPriority(nodes[childRightIndex], nodes[i])) {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}