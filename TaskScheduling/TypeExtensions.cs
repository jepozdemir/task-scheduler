using System;
using System.Linq;
using System.Reflection;

namespace TaskScheduling
{
    public static class TypeExtensions
    {
        public static object GetInstance(this string typeName, params object[] args)
        {
            var type = Type.GetType(typeName);
            return type.GetInstance(args);
        }

        public static object GetInstance(this Type type, params object[] args)
        {
            type = type.GetRealType();
            if (args == null || args.Length <= 0)
            {
                var constructor = type.GetConstructors().First();
                var parameters = constructor.GetParameters();

                if (!parameters.Any()) return Activator.CreateInstance(type);
                args = parameters.Select(p => p.ParameterType.GetInstance()).ToArray();
            }
            return Activator.CreateInstance(type, args);
        }
        
        public static Type GetRealType(this Type type)
        {
            return Assembly.GetAssembly(type).GetExportedTypes()
                .Where(type.IsAssignableFrom)
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .First();
        }
    }
}
