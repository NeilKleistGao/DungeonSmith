using System.Collections;
using System.Collections.Generic;

public class UnionSet
{
    private int[] parents;

    public UnionSet(int size)
    {
        parents = new int[size];
        for (int i = 0; i < size; i++)
        {
            parents[i] = i;
        }
    }

    public int Find(int x)
    {
        int px = parents[x];
        while (px != parents[px])
        {
            px = parents[px];
        }

        while (parents[x] != px)
        {
            int temp = parents[x];
            parents[x] = px;
            x = temp;
        }

        return px;
    }

    public bool Union(int x, int y) 
    {
        int px = Find(x), py = Find(y);
        if (px == py)
        {
            return false;
        }
        else 
        {
            parents[px] = py;
            return true;
        }
    }
}
