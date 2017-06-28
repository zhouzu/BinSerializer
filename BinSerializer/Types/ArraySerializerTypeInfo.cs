﻿using System;
using System.Reflection;
using System.Security;
using System.Text;

namespace ThirtyNineEighty.BinarySerializer.Types
{
  sealed class ArraySerializerTypeInfo : SerializerTypeInfo
  {
    [SecurityCritical]
    public ArraySerializerTypeInfo(BinTypeDescription description, BinTypeVersion version, BinTypeProcess process) 
      : base(description, version, process)
    {
      if (description.Type != typeof(Array).GetTypeInfo())
        throw new ArgumentException("Type must be an array.");

      if (!string.Equals(description.TypeId, SerializerTypes.ArrayToken, StringComparison.Ordinal))
        throw new ArgumentException("TypeId must be an array.");
    }

    // Must be called under SerializerTypes read lock
    [SecuritySafeCritical]
    public override TypeInfo GetType(string notNormalizedTypeId)
    {
      var elementTypeIdStartIdx = notNormalizedTypeId.IndexOf('[');
      var elementTypeId = notNormalizedTypeId.Substring(elementTypeIdStartIdx + 1, notNormalizedTypeId.Length - elementTypeIdStartIdx - 2);
      var elementType = SerializerTypes.GetTypeImpl(elementTypeId);

      return elementType
        .MakeArrayType()
        .GetTypeInfo();
    }

    // Must be called under SerializerTypes read lock
    [SecuritySafeCritical]
    public override string GetTypeId(TypeInfo notNormalizedType)
    {
      var elementType = notNormalizedType.GetElementType();
      var elementTypeId = SerializerTypes.GetTypeIdImpl(elementType);

      var builder = new StringBuilder();
      builder.Append(TypeId);
      builder.Append('[');
      builder.Append(elementTypeId);
      builder.Append(']');

      return builder.ToString();
    }
  }
}
