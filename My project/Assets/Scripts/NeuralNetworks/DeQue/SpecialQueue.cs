using UnityEngine;

public class SpecialQueue : Deque
{
    public SpecialQueue(int maxSize) : base(maxSize)
    {

    }

    // get a number of random elements to be taken from the queue and then clear the queue
    public NeuralState[] ClearAtRandom(int objNumber)
    {
        int[] randoms = new int[objNumber + 1];
        NeuralState[] result = new NeuralState[objNumber];
        QueueIterator iterator = new QueueIterator(queue);

        // get a random index from the zone of the queue ensuring that the randoms are more spread out
        randoms[0] = 0;
        for (int i = 1; i < objNumber; i++)
        {
            randoms[i] = Random.Range(queue.Count / objNumber * (i - 1), queue.Count / objNumber * (i));
        }
        // insert the random states into the result array
        int counter = 0;
        for (int i = 1; i < randoms.Length; i++)
        {
            iterator.Step(randoms[i] - randoms[i - 1]);
            result[counter] = iterator.Current;
            counter++;
        }

        this.Clear();

        return result;
    }

    // Clear the Queue
    public void Clear()
    {
        queue.Clear();
    }


}
