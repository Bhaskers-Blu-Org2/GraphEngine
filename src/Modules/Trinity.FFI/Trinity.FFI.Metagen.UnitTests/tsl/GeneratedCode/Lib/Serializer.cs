#pragma warning disable 162,168,649,660,661,1522

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Trinity;
using Trinity.TSL;
using Trinity.TSL.Lib;
namespace Trinity.Extension
{
    /// <summary>
    /// Provides facilities for serializing data to Json strings.
    /// </summary>
    public class Serializer
    {
        [ThreadStatic]
        static StringBuilder s_stringBuilder;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void s_ensure_string_builder()
        {
            if (s_stringBuilder == null)
                s_stringBuilder = new StringBuilder();
            else
                s_stringBuilder.Clear();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// Serializes a int object to Json string.
        /// </summary>
        /// <param name="value">The target object to be serialized.</param>
        /// <returns>The serialized Json string.</returns>
        public static string ToString(int value)
        {
            s_ensure_string_builder();
            ToString_impl(value, s_stringBuilder, in_json: false);
            return s_stringBuilder.ToString();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// Serializes a string object to Json string.
        /// </summary>
        /// <param name="value">The target object to be serialized.</param>
        /// <returns>The serialized Json string.</returns>
        public static string ToString(string value)
        {
            s_ensure_string_builder();
            ToString_impl(value, s_stringBuilder, in_json: false);
            return s_stringBuilder.ToString();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// Serializes a List<int> object to Json string.
        /// </summary>
        /// <param name="value">The target object to be serialized.</param>
        /// <returns>The serialized Json string.</returns>
        public static string ToString(List<int> value)
        {
            s_ensure_string_builder();
            ToString_impl(value, s_stringBuilder, in_json: false);
            return s_stringBuilder.ToString();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// Serializes a List<string> object to Json string.
        /// </summary>
        /// <param name="value">The target object to be serialized.</param>
        /// <returns>The serialized Json string.</returns>
        public static string ToString(List<string> value)
        {
            s_ensure_string_builder();
            ToString_impl(value, s_stringBuilder, in_json: false);
            return s_stringBuilder.ToString();
        }
        
        /// <summary>
        /// Serializes a C1 object to Json string.
        /// </summary>
        /// <param name="value">The target cell object to be serialized.</param>
        /// <returns>The serialized Json string.</returns>
        public static string ToString(C1 cell)
        {
            s_ensure_string_builder();
            s_stringBuilder.Append('{');
            s_stringBuilder.AppendFormat("\"CellId\":{0}", cell.CellId);
            
            {
                
                    s_stringBuilder.Append(',');
                    s_stringBuilder.Append("\"i\":");
                    ToString_impl(cell.i, s_stringBuilder, in_json: true);
                    
            }
            
            s_stringBuilder.Append('}');
            return s_stringBuilder.ToString();
        }
        
        /// <summary>
        /// Serializes a C2 object to Json string.
        /// </summary>
        /// <param name="value">The target cell object to be serialized.</param>
        /// <returns>The serialized Json string.</returns>
        public static string ToString(C2 cell)
        {
            s_ensure_string_builder();
            s_stringBuilder.Append('{');
            s_stringBuilder.AppendFormat("\"CellId\":{0}", cell.CellId);
            
            {
                
                    s_stringBuilder.Append(',');
                    s_stringBuilder.Append("\"s\":");
                    ToString_impl(cell.s, s_stringBuilder, in_json: true);
                    
            }
            
            {
                
                if (cell.ls != null)
                {
                    
                    s_stringBuilder.Append(',');
                    s_stringBuilder.Append("\"ls\":");
                    ToString_impl(cell.ls, s_stringBuilder, in_json: true);
                    
                }
                
            }
            
            s_stringBuilder.Append('}');
            return s_stringBuilder.ToString();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ToString_impl(int value, StringBuilder str_builder, bool in_json)
        {
            
            {
                
                {
                    str_builder.Append(value);
                }
                
            }
            
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ToString_impl(string value, StringBuilder str_builder, bool in_json)
        {
            
            if (in_json)
            {
                str_builder.Append(JsonStringProcessor.escape(value));
            }
            else
            {
                str_builder.Append(value);
            }
            
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ToString_impl(List<int> value, StringBuilder str_builder, bool in_json)
        {
            
            {
                str_builder.Append('[');
                bool first = true;
                foreach (var element in value)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        str_builder.Append(',');
                    }
                    ToString_impl(element, str_builder, in_json:true);
                }
                str_builder.Append(']');
            }
            
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ToString_impl(List<string> value, StringBuilder str_builder, bool in_json)
        {
            
            {
                str_builder.Append('[');
                bool first = true;
                foreach (var element in value)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        str_builder.Append(',');
                    }
                    ToString_impl(element, str_builder, in_json:true);
                }
                str_builder.Append(']');
            }
            
        }
        
        #region mute
        
        #endregion
    }
}

#pragma warning restore 162,168,649,660,661,1522
