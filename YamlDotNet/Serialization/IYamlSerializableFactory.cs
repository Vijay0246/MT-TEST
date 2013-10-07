using System;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// A factory of <see cref="IYamlSerializable"/>
	/// </summary>
	public interface IYamlSerializableFactory
	{
		/// <summary>
		/// Try to create a <see cref="IYamlSerializable"/> or return null if not supported for a particular .NET type.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="type">The type.</param>
		/// <returns>If supported, return an instance of <see cref="IYamlSerializable"/> else return <c>null</c>.</returns>
		IYamlSerializable TryCreate(SerializerContext context, Type type);
	}
}