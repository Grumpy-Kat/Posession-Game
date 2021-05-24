using System;
using System.Collections;
using System.Collections.Generic;

namespace Priority_Queue {
    public sealed class PriorityQueue<T> : IPriorityQueue<T> {
        private class Node : FastPriorityQueueNode {
            public T Data { get; private set; }

            public Node(T data) {
                Data = data;
            }
        }

        private const int INITIAL_QUEUE_SIZE = 10;
        private readonly FastPriorityQueue<Node> queue;

        public PriorityQueue() {
            queue = new FastPriorityQueue<Node>(INITIAL_QUEUE_SIZE);
        }

        private Node GetExistingNode(T item) {
            var comparer = EqualityComparer<T>.Default;
            foreach (var node in queue) {
                if (comparer.Equals(node.Data, item)) {
                    return node;
                }
            }
            throw new InvalidOperationException("Item cannot be found in queue: " + item);
        }

        public int Count {
            get {
                lock (queue) {
                    return queue.Count;
                }
            }
        }

        public T First {
            get {
                lock (queue) {
                    if (queue.Count <= 0) {
                        throw new InvalidOperationException("Cannot call .First on an empty queue");
                    }
                    Node first = queue.First;
                    return (first != null ? first.Data : default(T));
                }
            }
        }

        public void Clear() {
            lock (queue) {
                queue.Clear();
            }
        }

        public bool Contains(T item) {
            lock (queue) {
                var comparer = EqualityComparer<T>.Default;
                foreach (var node in queue) {
                    if (comparer.Equals(node.Data, item)) {
                        return true;
                    }
                }
                return false;
            }
        }

        public T Dequeue() {
            lock (queue) {
                if (queue.Count <= 0) {
                    throw new InvalidOperationException("Cannot call Dequeue() on an empty queue");
                }
                Node node = queue.Dequeue();
                return node.Data;
            }
        }

        public void Enqueue(T item, double priority) {
            lock (queue) {
                Node node = new Node(item);
                if (queue.Count == queue.MaxSize) {
                    queue.Resize(queue.MaxSize * 2 + 1);
                }
                queue.Enqueue(node, priority);
            }
        }

        public void Remove(T item) {
            lock (queue) {
                try {
                    queue.Remove(GetExistingNode(item));
                } catch (InvalidOperationException e) {
                    throw new InvalidOperationException("Cannot call Remove() on a node which is not enqueued: " + item, e);
                }
            }
        }

        public void UpdatePriority(T item, double priority) {
            lock (queue) {
                try {
                    Node updateMe = GetExistingNode(item);
                    queue.UpdatePriority(updateMe, priority);
                } catch (InvalidOperationException e) {
                    throw new InvalidOperationException("Cannot call UpdatePriority() on a node which is not enqueued: " + item, e);
                }
            }
        }

        public IEnumerator<T> GetEnumerator() {
            List<T> queueData = new List<T>();
            lock (queue) {
                // Copy to a separate list to not put yield return inside a lock
                foreach (var node in queue) {
                    queueData.Add(node.Data);
                }
            }
            return queueData.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public bool IsValidQueue() {
            lock (queue) {
                return queue.IsValidQueue();
            }
        }
    }
}