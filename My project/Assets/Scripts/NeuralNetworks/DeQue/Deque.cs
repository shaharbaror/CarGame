using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deque : IEnumerable<NeuralState>
{
    protected int maxSize;
    protected Queue<NeuralState> queue;

    public Deque(int maxSize)
    {
        this.maxSize = maxSize;
        queue = new Queue<NeuralState>(maxSize);
    }

    public void Push(NeuralState value)
    {
        if (queue.Count == maxSize)
        {
            queue.Dequeue();
        }
        queue.Enqueue(value);
    }

    public NeuralState Pop()
    {
        return queue.Dequeue();
    }

    //public NeuralState[] ClearAtRandom(int objNumber)
    //{
    //    int[] randoms = new int[objNumber + 1];
    //    NeuralState[] result = new NeuralState[objNumber];
    //    QueueIterator iterator = new QueueIterator(queue);

    //    // get a random index from the zone of the queue ensuring that the randoms are more spread out
    //    randoms[0] = 0;
    //    for (int i = 1; i < objNumber; i++)
    //    {
    //        randoms[i] = Random.Range(queue.Count / objNumber * (i - 1), queue.Count / objNumber * (i));
    //    }
    //    // insert the random states into the result array
    //    int counter = 0;
    //    for (int i = 1; i < randoms.Length; i++)
    //    {
    //        iterator.Step(randoms[i] - randoms[i - 1]);
    //        result[counter] = iterator.Current;
    //        counter++;
    //    }


    //    return result;
    //}

    public IEnumerator<NeuralState> GetEnumerator()
    {
        return new QueueIterator(queue);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

}


