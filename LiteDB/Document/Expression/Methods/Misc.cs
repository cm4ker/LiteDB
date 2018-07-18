﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using static LiteDB.ZipExtensions;

namespace LiteDB
{
    internal partial class BsonExpressionMethods
    {
        /// <summary>
        /// Parse a JSON string into a new BsonValue. Support multiple values (string only)
        /// JSON('{a:1}') = {a:1}
        /// </summary>
        public static IEnumerable<BsonValue> JSON(IEnumerable<BsonValue> values)
        {
            foreach (var str in values.Where(x => x.IsString))
            {
                BsonValue value = null;
                var isJson = false;

                try
                {
                    value = JsonSerializer.Deserialize(str);
                    isJson = true;
                }
                catch (LiteException ex) when (ex.ErrorCode == LiteException.UNEXPECTED_TOKEN)
                {
                }

                if (isJson) yield return value;
            }
        }

        /// <summary>
        /// Extend source document with other document. Copy all field from extend to source. Source document will be modified.
        /// EXTEND($, {a: 2}) = {_id:1, a: 2}
        /// </summary>
        public static IEnumerable<BsonValue> EXTEND(IEnumerable<BsonValue> source, IEnumerable<BsonValue> extend)
        {
            foreach (var value in ZipValues(source, extend))
            {
                if (!value.First.IsDocument) continue;
                if (!value.Second.IsDocument) continue;

                var dest = value.First.AsDocument;

                value.Second.AsDocument.CopyTo(dest);

                yield return dest;
            }
        }

        /// <summary>
        /// Convert an array into IEnuemrable of values.
        /// ITEMS([1, 2, null]) = 1, 2, null
        /// </summary>
        public static IEnumerable<BsonValue> ITEMS(IEnumerable<BsonValue> array)
        {
            foreach (var arr in array.Where(x => x.IsArray).Select(x => x.AsArray))
            {
                foreach (var value in arr)
                {
                    yield return value;
                }
            }
        }

        /// <summary>
        /// Get all KEYS names from a document. Support multiple values (document only)
        /// </summary>
        public static IEnumerable<BsonValue> KEYS(IEnumerable<BsonValue> values)
        {
            foreach (var value in values.Where(x => x.IsDocument))
            {
                foreach(var key in value.AsDocument.Keys)
                {
                    yield return key;
                }
            }
        }

        /// <summary>
        /// Conditional IF statment. If condition are true, returns TRUE value, otherwise, FALSE value. Support multiple values (only string)
        /// </summary>
        public static IEnumerable<BsonValue> IIF(IEnumerable<BsonValue> condition, IEnumerable<BsonValue> ifTrue, IEnumerable<BsonValue> ifFalse)
        {
            foreach (var value in ZipValues(condition, ifTrue, ifFalse).Where(x => x.First.IsBoolean))
            {
                yield return value.First.AsBoolean ? value.Second : value.Third;
            }
        }

        /// <summary>
        /// Return length of variant value (valid only for String, Binary, Array or Document [keys])
        /// </summary>
        public static IEnumerable<BsonValue> LENGTH(IEnumerable<BsonValue> values)
        {
            foreach (var value in values)
            {
                if (value.IsString) yield return value.AsString.Length;
                else if (value.IsBinary) yield return value.AsBinary.Length;
                else if (value.IsArray) yield return value.AsArray.Count;
                else if (value.IsDocument) yield return value.AsDocument.Keys.Count;
            }
        }
    }
}
