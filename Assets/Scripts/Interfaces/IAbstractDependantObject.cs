using UnityEngine;
using System.Collections;

public interface IAbstractDependantObject
{
    int m_priority { get; set; }
    void CompareWithAbstract(RoomAbstract roomAbstract);
}
