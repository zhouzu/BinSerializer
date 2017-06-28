﻿using System.IO;
using System.Reflection;
using System.Security;
using ThirtyNineEighty.BinarySerializer.Types;

#if NET45
using System.Security.Permissions;
#endif

namespace ThirtyNineEighty.BinarySerializer
{
  [SecuritySafeCritical]
  public static class BinSerializer
  {
    [SecuritySafeCritical]
#if NET45
    [ReflectionPermission(SecurityAction.Assert, Unrestricted = true)]
    [SecurityPermission(SecurityAction.Assert, ControlEvidence = true)]
#endif
    public static void Serialize<T>(Stream stream, T obj)
    {
      BinSerializer<T>.Serialize(stream, obj);
    }

    [SecuritySafeCritical]
#if NET45
    [ReflectionPermission(SecurityAction.Assert, Unrestricted = true)]
    [SecurityPermission(SecurityAction.Assert, ControlEvidence = true)]
#endif
    public static T Deserialize<T>(Stream stream)
    {
      return BinSerializer<T>.Deserialize(stream);
    }
  }

  [SecuritySafeCritical]
  static class BinSerializer<T>
  {
    private static readonly Writer<T> CachedWriter;
    private static readonly Reader<T> CachedReader;

    [SecuritySafeCritical]
    static BinSerializer()
    {
      CachedWriter = GetWriterInvoker();
      CachedReader = GetReaderInvoker();
    }

    [SecurityCritical]
    private static Writer<T> GetWriterInvoker()
    {
      var typeInfo = typeof(T).GetTypeInfo();
      return typeInfo.IsValueType || typeInfo.IsSealed
        ? SerializerBuilder.CreateWriter<T>(typeof(T))
        : null;
    }

    [SecurityCritical]
    private static Reader<T> GetReaderInvoker()
    {
      var typeInfo = typeof(T).GetTypeInfo();
      return typeInfo.IsValueType || typeInfo.IsSealed
        ? SerializerBuilder.CreateReader<T>(typeof(T))
        : null;
    }

    [SecuritySafeCritical]
    public static void Serialize(Stream stream, T obj)
    {
      using (new RefWriterWatcher(true))
      {
        if (CachedWriter != null)
          CachedWriter(stream, obj);
        else
        {
          var type = ReferenceEquals(obj, null) ? typeof(T) : obj.GetType();
          var writer = SerializerBuilder.CreateWriter<T>(type);
          writer(stream, obj);
        }
      }
    }

    [SecuritySafeCritical]
    public static T Deserialize(Stream stream)
    {
      using (new RefReaderWatcher(true))
      {
        BSDebug.TraceStart("Start read of ...");

        var typeId = stream.ReadString();   

        BSDebug.TraceStart("... " + typeId);

        if (CachedReader != null)
          return CachedReader(stream);

        var type = SerializerTypes.GetType(typeId);
        var reader = SerializerBuilder.CreateReader<T>(type);
        return reader(stream);
      }
    }
  }
}
