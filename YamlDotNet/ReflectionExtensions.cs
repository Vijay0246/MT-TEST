﻿// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry and contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace YamlDotNet
{
    internal static class ReflectionExtensions
    {
        public static Type? BaseType(this Type type)
        {
            return type.GetTypeInfo().BaseType;
        }

        public static bool IsValueType(this Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static bool IsInterface(this Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }

        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        /// <summary>
        /// Determines whether the specified type has a default constructor.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="allowPrivateConstructors">Whether to include private constructors</param>
        /// <returns>
        ///     <c>true</c> if the type has a default constructor; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasDefaultConstructor(
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
            this Type type, bool allowPrivateConstructors)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;

            if (allowPrivateConstructors)
            {
                bindingFlags |= BindingFlags.NonPublic;
            }

            return type.IsValueType || type.GetConstructor(bindingFlags, null, Type.EmptyTypes, null) != null;
        }

        public static TypeCode GetTypeCode(this Type type)
        {
            var isEnum = type.IsEnum();
            if (isEnum)
            {
                type = Enum.GetUnderlyingType(type);
            }

            if (type == typeof(bool))
            {
                return TypeCode.Boolean;
            }
            else if (type == typeof(char))
            {
                return TypeCode.Char;
            }
            else if (type == typeof(sbyte))
            {
                return TypeCode.SByte;
            }
            else if (type == typeof(byte))
            {
                return TypeCode.Byte;
            }
            else if (type == typeof(short))
            {
                return TypeCode.Int16;
            }
            else if (type == typeof(ushort))
            {
                return TypeCode.UInt16;
            }
            else if (type == typeof(int))
            {
                return TypeCode.Int32;
            }
            else if (type == typeof(uint))
            {
                return TypeCode.UInt32;
            }
            else if (type == typeof(long))
            {
                return TypeCode.Int64;
            }
            else if (type == typeof(ulong))
            {
                return TypeCode.UInt64;
            }
            else if (type == typeof(float))
            {
                return TypeCode.Single;
            }
            else if (type == typeof(double))
            {
                return TypeCode.Double;
            }
            else if (type == typeof(decimal))
            {
                return TypeCode.Decimal;
            }
            else if (type == typeof(DateTime))
            {
                return TypeCode.DateTime;
            }
            else if (type == typeof(string))
            {
                return TypeCode.String;
            }
            else
            {
                return TypeCode.Object;
            }
        }

        public static bool IsDbNull(this object value)
        {
            return value.GetType().FullName == "System.DBNull";
        }

        private static readonly Func<PropertyInfo, bool> IsInstance = (PropertyInfo property) => !(property.GetMethod ?? property.SetMethod).IsStatic;
        private static readonly Func<PropertyInfo, bool> IsInstancePublic = (PropertyInfo property) => IsInstance(property) && (property.GetMethod ?? property.SetMethod).IsPublic;

        public static IEnumerable<PropertyInfo> GetProperties(
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces |
                                        DynamicallyAccessedMemberTypes.PublicProperties |
                                        DynamicallyAccessedMemberTypes.NonPublicProperties)]
#endif
            this Type type, bool includeNonPublic)
        {
            var predicate = includeNonPublic ? IsInstance : IsInstancePublic;

            return type.IsInterface()
                ? (new Type[] { type })
                    .Concat(type.GetInterfaces())
                    .SelectMany((
#if NET6_0_OR_GREATER
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)]
#endif
                i) => i.GetRuntimeProperties().Where(predicate))
                : type.GetRuntimeProperties().Where(predicate);
        }

        public static IEnumerable<PropertyInfo> GetPublicProperties(
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces |
                                        DynamicallyAccessedMemberTypes.PublicProperties |
                                        DynamicallyAccessedMemberTypes.NonPublicProperties)]
#endif
            this Type type) => GetProperties(type, false);

        public static IEnumerable<FieldInfo> GetPublicFields(
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields |DynamicallyAccessedMemberTypes.NonPublicFields)]
#endif
            this Type type)
        {
            return type.GetRuntimeFields().Where(f => !f.IsStatic && f.IsPublic);
        }

        public static IEnumerable<MethodInfo> GetPublicStaticMethods(
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)]
#endif
            this Type type)
        {
            return type.GetRuntimeMethods()
                .Where(m => m.IsPublic && m.IsStatic);
        }

        public static MethodInfo? GetPublicStaticMethod(
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)]
#endif
            this Type type, string name, params Type[] parameterTypes)
        {
            return type.GetRuntimeMethods()
                .FirstOrDefault(m =>
                {
                    if (m.IsPublic && m.IsStatic && m.Name.Equals(name))
                    {
                        var parameters = m.GetParameters();
                        return parameters.Length == parameterTypes.Length
                            && parameters.Zip(parameterTypes, (pi, pt) => pi.ParameterType == pt).All(r => r);
                    }
                    return false;
                });
        }

        public static Attribute[] GetAllCustomAttributes<TAttribute>(this PropertyInfo member)
        {
            return Attribute.GetCustomAttributes(member, typeof(TAttribute), true);
        }
    }
}
