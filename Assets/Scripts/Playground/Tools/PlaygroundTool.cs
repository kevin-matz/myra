using UnityEngine;
using UnityEngine.UI;

namespace Playground.Tools
{
    public abstract class PlaygroundTool : MonoBehaviour
    {
        public Button GuiToolButton;
        
        public string Id { get; private set; }

        protected PlaygroundTool(string id)
        {
            Id = id;
        }

        public bool IsActive;
        
        protected abstract void UpdateInternal();
        protected abstract void UpdateInternalWhenActive();

        public virtual void OnActivate()
        {
        }

        public virtual void OnDeactivate()
        {
        }

        private void Update()
        {
            UpdateInternal();
            
            if (!IsActive)
                return;

            UpdateInternalWhenActive();
        }
    }
}