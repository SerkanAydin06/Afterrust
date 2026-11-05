using UnityEngine;

namespace Afterrust
{
    /// <summary>
    /// Records which automatic scene update (see AfterrustSceneSync in the Editor folder) the
    /// saved scene already contains. Editor-only bookkeeping; does nothing at runtime.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SceneBuildInfo : MonoBehaviour
    {
        public int version;
    }
}
