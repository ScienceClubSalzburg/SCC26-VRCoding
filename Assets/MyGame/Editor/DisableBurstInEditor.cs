#if UNITY_EDITOR
using Unity.Burst;
using UnityEditor;

[InitializeOnLoad]
public static class DisableBurstInEditor
{
    static DisableBurstInEditor()
    {
        BurstCompiler.Options.EnableBurstCompilation = false;
    }
}
#endif
