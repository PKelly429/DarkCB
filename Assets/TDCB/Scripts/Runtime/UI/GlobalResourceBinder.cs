using System.Collections;
using System.Collections.Generic;
using DataBinding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TDCB
{
    public class GlobalResourceBinder : AbstractBinder
    {
        [SerializeField] private ResourceType resource;
        
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text currentSupply;
        [SerializeField] private TMP_Text productionRate;

        private Resource _boundResource;
        
        public void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            icon.sprite = SceneReferences.Instance.resourceManager.GetResourceIcon(resource);
            Bind(SceneReferences.Instance.resourceManager.GetResource(resource));
        }

        public override void Bind(object obj)
        {
            Unbind();
            
            _boundResource = (Resource)obj;
            
            _boundResource.Value.onValueChanged += OnStockpileValueChanged;
            _boundResource.Max.onValueChanged += OnStockpileValueChanged;
            
            _boundResource.ProductionRate.onValueChanged += OnProductionValueChanged;
            
            productionRate.gameObject.SetActive(_boundResource.stockpiles);
            OnStockpileValueChanged();
            OnProductionValueChanged();
        }

        private void OnStockpileValueChanged()
        {
            if (_boundResource.hasMaximum)
            {
                currentSupply.text = $"{_boundResource.Value.GetValue()}/{_boundResource.Max.GetValue()}";
            }
            else
            {
                currentSupply.text = $"{_boundResource.Value.GetValue()}";
            }
        }
        
        private void OnProductionValueChanged()
        {
            if (_boundResource.ProductionRate > 0)
            {
                productionRate.text = $"+{_boundResource.ProductionRate.GetValue()}";   
            }
            else
            {
                productionRate.text = $"{_boundResource.ProductionRate.GetValue()}";
            }
        }

        public override void Unbind()
        {
            if (_boundResource != null)
            {
                _boundResource.Value.onValueChanged -= OnStockpileValueChanged;
                _boundResource.Max.onValueChanged -= OnStockpileValueChanged;
                
                _boundResource.ProductionRate.onValueChanged -= OnProductionValueChanged;
            }
        }

        public override void DebugBinder()
        {
        }
    }
}
