using Unity.Jobs;
using UnityEngine;

namespace henningboat.CubeMarching.Runtime.NewDistanceFieldResolverPrototype
{
    [ExecuteInEditMode]
    public class NewCullingTester:MonoBehaviour{
        private void Update()
        {
            JRecursivelyResolveDistanceField job = new JRecursivelyResolveDistanceField();
            job.Schedule().Complete();
        }
    }
}