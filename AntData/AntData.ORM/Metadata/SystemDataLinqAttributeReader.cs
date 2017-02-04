﻿#if !NETSTANDARD
using System;
using System.Linq;
using System.Reflection;

namespace AntData.ORM.Metadata
{
	using Common;
	using Mapping;

	public class SystemDataLinqAttributeReader : IMetadataReader
	{
		readonly AttributeReader _reader = new AttributeReader();

		public T[] GetAttributes<T>(Type type, bool inherit)
			where T : Attribute
		{
			if (typeof(T) == typeof(TableAttribute))
			{
				var ta = _reader.GetAttributes<System.Data.Linq.Mapping.TableAttribute>   (type, inherit);
				var da = _reader.GetAttributes<System.Data.Linq.Mapping.DatabaseAttribute>(type, inherit);

				var t = ta.Length == 1 ? ta[0] : null;
				var d = da.Length == 1 ? da[0] : null;

				if (t != null || d != null)
				{
					var attr = new TableAttribute();

					if (t != null)
					{
						var name = t.Name;

						if (name != null)
						{
							var names = name.Replace("[", "").Replace("]", "").Split('.');

							switch (names.Length)
							{
								case 0  : break;
								case 1  : attr.Name = names[0]; break;
								case 2  :
									attr.Name   = names[0];
									attr.Schema = names[1];
									break;
								default :
									throw new MetadataException(string.Format(
										"Invalid table name '{0}' of type '{1}'",
										name, type.FullName));
							}
						}
					}

					if (d != null)
						attr.Database = d.Name;

					return new[] { (T)(Attribute)attr };
				}
			}

			return Array<T>.Empty;
		}

		public T[] GetAttributes<T>(MemberInfo memberInfo, bool inherit)
			where T : Attribute
		{
			if (typeof(T) == typeof(ColumnAttribute))
			{
				var attrs = _reader.GetAttributes<System.Data.Linq.Mapping.ColumnAttribute>(memberInfo, inherit);

				if (attrs.Length == 1)
				{
					var c = attrs[0];

					var attr = new ColumnAttribute
					{
						Name      = c.Name,
						DbType    = c.DbType,
						CanBeNull = c.CanBeNull,
						Storage   = c.Storage,
					};

					return new[] { (T)(Attribute)attr };
				}
			}
			else if (typeof(T) == typeof(AssociationAttribute))
			{
				var ta = _reader.GetAttributes<System.Data.Linq.Mapping.TableAttribute>(memberInfo.DeclaringType, inherit);

				if (ta.Length == 1)
				{
					return _reader
						.GetAttributes<System.Data.Linq.Mapping.AssociationAttribute>(memberInfo, inherit)
						.Select(a => (T)(Attribute)new AssociationAttribute { ThisKey = a.ThisKey, OtherKey = a.OtherKey, Storage = a.Storage })
						.ToArray();
				}
			}

			return Array<T>.Empty;
		}
	}
}
#endif