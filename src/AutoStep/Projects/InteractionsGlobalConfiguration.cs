using System.Reflection;
using AutoStep.Definitions.Interaction;
using AutoStep.Language.Interaction;

namespace AutoStep.Projects
{
    public class InteractionsGlobalConfiguration
    {
        internal MethodTable MethodTable { get; } = new MethodTable();

        public void AddOrReplaceMethod(InteractionMethod method)
        {
            MethodTable.Set(method);
        }

        public void AddMethods<TMethodsClass>()
        {
            var methods = typeof(TMethodsClass).GetMethods();

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<InteractionMethodAttribute>();

                if (attr is object)
                {
                    MethodTable.Set(new ClassBackedInteractionMethod(attr.Name, typeof(TMethodsClass), method));
                }
            }
        }
    }
}
