using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueIterator : IEnumerator<NeuralState>
{
    private Queue<NeuralState> queue;
    private IEnumerator<NeuralState> enumerator;

    public QueueIterator(Queue<NeuralState> queue)
    {
        this.queue = queue;
        this.enumerator = queue.GetEnumerator();
    }

    public NeuralState Current => enumerator.Current;

    object IEnumerator.Current => Current;
    public void Reset()
    {
        enumerator.Reset();
    }
    public bool MoveNext()
    {
        return enumerator.MoveNext();
    }

    public bool Step(int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            if (!MoveNext())
            {
                return false;
            }
        }
        return true;
    }
    public void Dispose()
    {
        enumerator.Dispose();
    }


}
