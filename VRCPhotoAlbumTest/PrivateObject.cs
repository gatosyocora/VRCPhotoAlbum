using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace VRCPhotoAlbumTest
{
    public class PrivateObject
    {
        private readonly object _obj;
        public PrivateObject(object obj)
        {
            _obj = obj;
        }

        public object GetProperty(string memberName)
        {
            var classType = _obj.GetType();
            var property = classType.GetProperty(memberName, 
                                BindingFlags.Public | BindingFlags.NonPublic |
                                BindingFlags.Instance);
            return property.GetValue(_obj);
        }

        public void SetInvokeMember(string memberName, object data)
        {
            var type = _obj.GetType();
            var inst = Activator.CreateInstance(type);
            type.InvokeMember(memberName, BindingFlags.SetProperty, null, inst, new object[] { data });
        }

        public object Invoke(string methodName, params object[] args)
        {
            var type = _obj.GetType();
            var bindingFlags = BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance;
            try
            {
                return type.InvokeMember(methodName, bindingFlags, null, _obj, args);
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }
    }
}
