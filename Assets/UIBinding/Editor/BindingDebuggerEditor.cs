using UnityEditor;
using UnityEngine;

namespace DataBinding.Editor
{
    public class BindingDebuggerEditor
    {
        [MenuItem("Tools/DataBinding/Find Broken Bindings", false, 1)]
        private static void FindMissingRefs()
        {
            AbstractBinder[] allBinders = GameObject.FindObjectsOfType<AbstractBinder>();

            foreach (var binder in allBinders)
            {
                binder.DebugBinder();
            }
            
            Debug.Log("Checked all binders in current scene.");
        }
    }
}
