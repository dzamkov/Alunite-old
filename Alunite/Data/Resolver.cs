using System;
using System.Collections.Generic;
using System.Reflection;

namespace Alunite
{
    /// <summary>
    /// An object that is able to resolve a problem with an object of a certain type. Objects implementing this interface should
    /// include a "Resolve" method which takes a parameter of any type (even a type defined with a generic argument) and returns an object
    /// of the given result type. When resolving, the object will be given to the resolver only if it can fit in the parameter for the "Resolve" method.
    /// </summary>
    public interface IResolver<TResult>
    {

    }

    /// <summary>
    /// Contains functions related to type-dependant resolving and resolvers.
    /// </summary>
    public static class Resolver
    {
        /// <summary>
        /// Tries resolving a problem with an object using the given resolver. Returns true if the resolver can accept the object and false otherwise.
        /// </summary>
        public static bool Resolve<TResult>(object Object, IResolver<TResult> Resolver, ref TResult Result)
        {
            MethodInfo resolve = Resolver.GetType().GetMethod("Resolve", BindingFlags.Instance | BindingFlags.Public);
            ParameterInfo[] pis = resolve.GetParameters();
            ParameterInfo pi = pis[0];

            if (resolve.IsGenericMethod)
            {
                Type[] args = resolve.GetGenericArguments();
                Type act = Object.GetType();
                Type pit = pi.ParameterType;
                if (Match(act, pit, args))
                {
                    try
                    {
                        Result = (TResult)resolve.MakeGenericMethod(args).Invoke(Resolver, new object[] { Object });
                    }
                    catch (ArgumentException)
                    {
                        // Constraint violation, oh well
                        return false;
                    }
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Sets the given type parameters so that the template type (which contains template parameters) matches the actual type, or
        /// returns false if this is not possible.
        /// </summary>
        public static bool Match(Type Actual, Type Template, Type[] Parameters)
        {
            if (Actual == Template)
            {
                return true;
            }
            if (Template.IsGenericParameter)
            {
                Parameters[Template.GenericParameterPosition] = Actual;
                return true;
            }
            if (!Actual.IsGenericType || !Template.IsGenericType)
            {
                return false;
            }
            if (Actual.GetGenericTypeDefinition() != Template.GetGenericTypeDefinition())
            {
                return false;
            }

            Type[] aargs = Actual.GetGenericArguments();
            Type[] targs = Template.GetGenericArguments();

            for (int t = 0; t < aargs.Length; t++)
            {
                Type aarg = aargs[t];
                Type targ = targs[t];
                if (!Match(aarg, targ, Parameters))
                {
                    return false;
                }
            }

            return true;
        }
    }
}