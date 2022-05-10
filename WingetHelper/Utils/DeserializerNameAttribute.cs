using System;

namespace WingetHelper.Utils
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DeserializerNameAttribute : Attribute
    {
        private readonly string _name;

        public DeserializerNameAttribute(string name)
        {
            _name = name;
        }

        public string Name => _name;
    }
}
