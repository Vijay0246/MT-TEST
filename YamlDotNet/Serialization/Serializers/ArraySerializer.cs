﻿using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Events;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Serializers
{
	internal class ArraySerializer : IYamlSerializable, IYamlSerializableFactory
	{
		public IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
		{
			return typeDescriptor is ArrayDescriptor ? this : null;
		}

		public virtual ValueResult ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			var reader = context.Reader;
			var arrayDescriptor = (ArrayDescriptor)typeDescriptor;

			bool isArray = value != null && value.GetType().IsArray;
			var arrayList = (IList)value;

			reader.Expect<SequenceStart>();
			int index = 0;
			if (isArray)
			{
				while (!reader.Accept<SequenceEnd>())
				{
					var node = reader.Peek<ParsingEvent>();
					if (index >= arrayList.Count)
					{
						throw new YamlException(node.Start, node.End, "Unable to deserialize array. Current number of elements [{0}] exceeding array size [{1}]".DoFormat(index, arrayList.Count));
					}

					var valueResult = context.ReadYaml(null, arrayDescriptor.ElementType);

					// Handle aliasing
					var localIndex = index;
					if (valueResult.IsAlias)
					{
						context.AddAliasBinding(valueResult.Alias, deferredValue => arrayList[localIndex] = deferredValue);
					}
					else
					{
						arrayList[localIndex] = valueResult.Value;
					}
					index++;
				}
			}
			else
			{
				var results = new List<ValueResult>();
				while (!reader.Accept<SequenceEnd>())
				{
					results.Add(context.ReadYaml(null, arrayDescriptor.ElementType));
				}

				// Handle aliasing
				arrayList = arrayDescriptor.CreateArray(results.Count);
				foreach (var valueResult in results)
				{
					var localIndex = index;
					if (valueResult.IsAlias)
					{
						context.AddAliasBinding(valueResult.Alias, deferredValue => arrayList[localIndex] = deferredValue);
					}
					else
					{
						arrayList[localIndex] = valueResult.Value;
					}
					index++;
				}
			}
			reader.Expect<SequenceEnd>();

			return new ValueResult(arrayList);
		}

		public void WriteYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			var typeOfValue = value.GetType();
			var expectedType = typeDescriptor != null ? typeDescriptor.Type : null;

			if (typeDescriptor == null)
			{
				typeDescriptor = context.FindTypeDescriptor(typeOfValue);
			}

			var arrayDescriptor = (ArrayDescriptor)typeDescriptor;

			var valueType = value.GetType();
			var arrayList = (IList) value;

			var tag = typeOfValue == expectedType ? null : context.TagFromType(typeOfValue);

			// Emit a Flow sequence or block sequence depending on settings 
			context.Writer.Emit(new SequenceStartEventInfo(value, valueType)
				{
					Tag = tag,
					Anchor = context.GetAnchor(),
					Style = arrayList.Count < context.Settings.LimitFlowSequence ? SequenceStyle.Flow : SequenceStyle.Block
				});

			foreach (var element in arrayList)
			{
				context.WriteYaml(element, arrayDescriptor.ElementType);
			}
			context.Writer.Emit(new SequenceEndEventInfo(value, valueType));
		}
	}
}