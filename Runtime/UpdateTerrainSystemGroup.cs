using Unity.Entities;
using UnityEngine;

[ExecuteAlways]
[UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = false)]
public class UpdateTerrainSystemGroup : ComponentSystemGroup
{
}