using System.Collections.Generic;

internal class ParamsList
{
    private List<object> objs;

    internal int Length
    {
        get
        {
            return this.objs.Count;
        }
    }

    internal ParamsList()
    {
        this.objs = new List<object>();
    }

    internal void Add(object o)
    {
        this.objs.Add(o);
    }

    internal void Remove(object o)
    {
        this.objs.Remove(o);
    }

    internal object Get(int index)
    {
        return this.objs[index];
    }

    internal object[] ToArray()
    {
        return this.objs.ToArray();
    }
}
