using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CodeRefractor.RuntimeBase.MiddleEnd;
using CodeRefractor.RuntimeBase.Runtime;

namespace CodeRefractor.RuntimeBase.Analyze.TypeTableIndices
{
    public static class TypesClosureLinker
    {
        public static List<Type> GetTypesClosure(List<MethodInterpreter> closure)
        {
            var typesSet = ScanMethodParameters(closure);
           
            bool isAdded;
            do
            {
                var toAdd = new HashSet<Type>(typesSet);
                foreach (var type in typesSet)
                {
                    var mappedType = type.ReversedType();
                    if (type.IsPrimitive)
                        continue;
                    var fields = mappedType.GetFields().ToList();
                    if (fields.Count == 0)
                        continue;
                    fields.AddRange(mappedType.GetFields(
                        BindingFlags.NonPublic 
                        | BindingFlags.Instance
                        | BindingFlags.Static));
                    foreach (var fieldInfo in fields)
                    {
                        var fieldType = fieldInfo.FieldType;
                        if (fieldType.IsInterface)
                            continue;
                        if (fieldType.IsSubclassOf(typeof(Array)))
                            fieldType = fieldType.GetElementType();
                        if (fieldType.IsPointer ||fieldType.IsByRef)
                            fieldType = fieldType.GetElementType();
                        
                        var  typeDesc = UsedTypeList.Set(type);
                        if(typeDesc == null)
                            continue;
                        toAdd.Add(fieldType);
                    }
                }
                isAdded = (toAdd.Count != typesSet.Count);
                typesSet = toAdd;

            } while (isAdded);
            var typesClosure = typesSet.Where(t => 
                IsRefClassType(t) && !t.IsInterface).ToList();
            foreach (var type in typesClosure)
            {
                UsedTypeList.Set(type);
            }
            var describedTypes = UsedTypeList.GetDescribedTypes();
            typesClosure = describedTypes
                .Where(typeDesc => typeDesc.ClrTypeCode == TypeCode.Object)
                .Where(typeDesc => !typeDesc.IsPointer && !typeDesc.ClrType.IsByRef)
                .Where(typeDesc =>
                    typeDesc.ClrType != typeof(void) &&
                    typeDesc.ClrType != typeof(IntPtr)
                )
                .Where(typeDescArray => !typeDescArray.ClrType.IsSubclassOf(typeof(Array)))
                .Where(typeDesc =>
                    typeDesc.ClrType.GetMappedType()==typeDesc.ClrType &&
                    !string.IsNullOrEmpty(typeDesc.ClrType.FullName)
                )
                .Select(typeDescr => typeDescr.ClrType)
                .ToList();
            var resultDictonary = new Dictionary<string, Type>();
            foreach (var typeDesc in typesClosure)
            {
                resultDictonary[typeDesc.ToCppMangling()] = typeDesc;
            }
            return resultDictonary.Values.ToList();
        }

        public static void SortTypeClosure(List<Type> types)
        {
            var typeComparer = new ClosureTypeComparer(types);
            typeComparer.Sort();
        }

        private static bool IsRefClassType(Type t)
        {
            var typeCode = Type.GetTypeCode(t);
            if (t.IsPrimitive)
                return false;
            if (t.HasElementType)
                return IsRefClassType(t.GetElementType());
            return true;
        }

        private static HashSet<Type> ScanMethodParameters(List<MethodInterpreter> closure)
        {
            var typesSet = new HashSet<Type>();
            foreach (var interpreter in closure)
            {
                var method = interpreter.Method;
                foreach (var parameter in method.GetParameters())
                {
                    var parameterType = parameter.ParameterType;

                    if (parameterType.IsSubclassOf(typeof(Array)))
                        continue;
                    if (parameterType.IsByRef)
                        parameterType = parameterType.GetElementType();
                    typesSet.Add(parameterType.GetReversedType());
                }
                typesSet.Add(method.DeclaringType.GetReversedType());
            }
            return typesSet;
        }

        private static void SortTypeDependencies(List<Type> typesClosure)
        {
            for (var i = 0; i < typesClosure.Count; i++)
            {
                var searchType = typesClosure[i];
                var typeDeps = TypeDependencies(searchType);
                if (typeDeps.Count == 0)
                    continue;
                foreach (var depType in typeDeps)
                {
                    var depSearchId = typesClosure.IndexOf(depType);
                    if (depSearchId <= i) continue;
                    if (searchType.Namespace == depType.Namespace)
                        continue;
                    typesClosure[depSearchId] = searchType;
                    typesClosure[i] = depType;
                    i = -1;
                    break;
                }
            }
        }

        private static List<Type> TypeDependencies(Type searchType)
        {
            var toAdd = new HashSet<Type>();

            var fields = searchType.GetFields().ToList();
            fields.AddRange(searchType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance));
            foreach (var fieldInfo in fields)
            {
                if (fieldInfo.FieldType.IsPrimitive)
                    continue;
                toAdd.Add(fieldInfo.FieldType);
            }
            return toAdd.ToList();
        }
    }
}