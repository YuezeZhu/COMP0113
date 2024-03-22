using System.Collections;
using System.Collections.Generic;

public class BaseTin
{
    protected float meltingPoint { get; set; }
    protected bool flux { get; set; }
    public BaseTin(float mp, bool f)
    {
        meltingPoint = mp;
        flux = f;
    }
}