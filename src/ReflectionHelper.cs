using System.Reflection;

namespace CreatureKilledBy;

public static class ReflectionHelper {
    public static T? GetField<T>(this object obj, string name) {
        return (T?)obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(obj);
    }

    public static T? GetProperty<T>(this object obj, string name) {
        return (T?)obj.GetType().GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(obj);
    }
}
