using System;
using System.Linq;
using System.Reflection;

namespace FrostAura.Services.Plutus.Data.Tests.Helpers
{
  public class PrivateObject
  {
    private readonly object o;

    public PrivateObject(object o)
    {
      this.o = o;
    }

    public T GetPrivateFieldValue<T>(string fieldName)
    {
      var field = o
        .GetType()
        .GetFields(BindingFlags.NonPublic |
                   BindingFlags.Instance)
        .FirstOrDefault(f => f.Name == fieldName);

      if (field == default) throw new ArgumentNullException(nameof(field));

      return (T)field
        .GetValue(o);
    }

    public object Invoke(string methodName, params object[] args)
    {
      var methodInfo = o.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
      if (methodInfo == null)
      {
        throw new Exception($"Method'{methodName}' not found is class '{o.GetType()}'");
      }
      return methodInfo.Invoke(o, args);
    }
  }
}
