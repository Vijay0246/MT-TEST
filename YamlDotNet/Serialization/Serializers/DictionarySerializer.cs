using System;
using System.Collections;
using System.Linq;
using YamlDotNet.Events;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Serializers
{
	internal class DictionarySerializer : ObjectSerializer
	{
		private readonly PureDictionarySerializer pureDictionarySerializer;

		public DictionarySerializer()
		{
			pureDictionarySerializer = new PureDictionarySerializer();
		}

		public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
		{
			return typeDescriptor is DictionaryDescriptor ? this : null;
		}

		protected override bool CheckIsSequence(ITypeDescriptor typeDescriptor)
		{
			var dictionaryDescriptor = (DictionaryDescriptor) typeDescriptor;

			// If the dictionary is pure, we can directly output a sequence instead of a mapping
			return dictionaryDescriptor.IsPureDictionary;
		}

		protected override void ReadItem(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			var dictionary = (IDictionary) thisObject;
			var dictionaryDescriptor = (DictionaryDescriptor) typeDescriptor;

			if (dictionaryDescriptor.IsPureDictionary)
			{
				var key = context.ReadYaml(null, dictionaryDescriptor.KeyType);
				var value = context.ReadYaml(null, dictionaryDescriptor.ValueType);
				dictionary.Add(key, value);
			}
			else
			{
				var keyEvent = context.Reader.Peek<Scalar>();
				if (keyEvent != null)
				{
					if (keyEvent.Value == context.Settings.SpecialCollectionMember)
					{
						context.Reader.Accept<Scalar>();
						pureDictionarySerializer.ReadYaml(context, thisObject, context.FindTypeDescriptor(thisObject.GetType()));
						return;
					}
				}

				base.ReadItem(context, thisObject, typeDescriptor);	
			}
		}

		public override void WriteItems(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			var dictionaryDescriptor = (DictionaryDescriptor)typeDescriptor;
			if (dictionaryDescriptor.IsPureDictionary)
			{
				pureDictionarySerializer.WriteItems(context, thisObject, typeDescriptor);
			}
			else
			{
				// Serialize Dictionary members
				foreach (var member in typeDescriptor.Members)
				{
					// Emit the key name
					WriteKey(context, member.Name);

					var memberValue = member.Get(thisObject);
					var memberType = member.Type;
					context.WriteYaml(memberValue, memberType);
				}

				WriteKey(context, context.Settings.SpecialCollectionMember);
				pureDictionarySerializer.WriteYaml(context, thisObject, context.FindTypeDescriptor(thisObject.GetType()));
			}
		}

		internal class PureDictionarySerializer : ObjectSerializer
		{
			protected override void ReadItem(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
			{
				var dictionary = (IDictionary)thisObject;
				var dictionaryDescriptor = (DictionaryDescriptor)typeDescriptor;

				var key = context.ReadYaml(null, dictionaryDescriptor.KeyType);
				var value = context.ReadYaml(null, dictionaryDescriptor.ValueType);
				dictionary.Add(key, value);
			}

			public override void WriteItems(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
			{
				var dictionary = (IDictionary) thisObject;
				var dictionaryDescriptor = (DictionaryDescriptor)typeDescriptor;

				var keys = dictionary.Keys;
				if (context.Settings.SortKeyForMapping)
				{
					var sortedKeys = keys.Cast<object>().ToList();
					sortedKeys.Sort((left, right) =>
						{
							if (left is IComparable && right is IComparable)
							{
								return ((IComparable) left).CompareTo(right);
							}
							return 0;
						});
					keys = sortedKeys;
				}

				var keyType = dictionaryDescriptor.KeyType;
				var valueType = dictionaryDescriptor.ValueType;
				foreach (var key in keys)
				{
					context.WriteYaml(key, keyType);
					context.WriteYaml(dictionary[key], valueType);
				}
			}
		}
	}
}