using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DataHolder
{
    public int capturesPerSecond = 0;
    public List<Vector3> positions = null;
    public List<Vector3> lookDirections = null;
}
