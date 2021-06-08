using System.Collections.Generic;

public class CommitSet<T>
{
    public readonly HashSet<T> listCommitted = new HashSet<T>();
    public readonly HashSet<T> listToAdd = new HashSet<T>();
    public readonly HashSet<T> listToRemove = new HashSet<T>();

    public void SlateForAdding(T textureRequest)
    {
        if (!listCommitted.Contains(textureRequest))
        {
            listToAdd.Add(textureRequest);
        }
        else
        {
            listToRemove.Remove(textureRequest);
        }
    }

    public void SlateForRemoval(T textureRequest)
    {
        if (listCommitted.Contains(textureRequest))
        {
            listToRemove.Add(textureRequest);
        }
        else
        {
            listToAdd.Remove(textureRequest);
        }
    }

    public void SlateForAdding(IEnumerable<T> textureRequests)
    {
        foreach (var textureRequest in textureRequests)
        {
            SlateForAdding(textureRequest);
        }
    }

    public void SlateForRemoval(IEnumerable<T> textureRequests)
    {
        foreach (var textureRequest in textureRequests)
        {
            SlateForRemoval(textureRequest);
        }
    }

    public void SlateForRemovalAll()
    {
        listToAdd.Clear();
        listToRemove.Clear();
        listToRemove.UnionWith(listCommitted);
    }

    public void Clear()
    {
        listCommitted.Clear();
        listToAdd.Clear();
        listToRemove.Clear();
    }

    public void Commit()
    {
        listCommitted.UnionWith(listToAdd);
        listCommitted.ExceptWith(listToRemove);
        listToAdd.Clear();
        listToRemove.Clear();
    }
}
