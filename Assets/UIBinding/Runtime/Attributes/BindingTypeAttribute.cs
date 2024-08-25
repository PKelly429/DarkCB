using System;

namespace DataBinding
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BindingTypeAttribute : Attribute
    {
        public Type bindingType;
        public BindingTypeAttribute(Type type)
        {
            bindingType = type;
        }
    }
}
