using System.Collections.Generic;

public class ParamsList
{
    private List<object> objs;

    public int Length
    {
        get
        {
            return this.objs.Count;
        }
    }

    public ParamsList()
    {
        this.objs = new List<object>();
    }

    public void Add(object o)
    {
        this.objs.Add(o);
    }

    public void Remove(object o)
    {
        this.objs.Remove(o);
    }

    public object Get(int index)
    {
        return this.objs[index];
    }

    public object[] ToArray()
    {
        return this.objs.ToArray<object>();
    }
}
