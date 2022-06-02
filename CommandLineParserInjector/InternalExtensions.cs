using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommandLineParserInjector;

internal static class InternalExtensions
{
	/// <summary>
    /// Converts a <see cref="Type"/> to its C# name. E.g., if you pass in typeof(<see cref="IEnumerable{int}"/>) this
    /// will return "IEnumerable<int>".
    /// This is useful mainly in exception error messages.
    /// </summary>
    /// <param name="type">The type to get a C# description of.</param>
    /// <returns>The string representation of a human-friendly type name</returns>
    public static string GetCSharpTypeName( this Type type, Type[] genericParameters, bool includeNamespaces = false ) {
	    if ( type == typeof( long ) ) {
		    return "long";
	    }

	    if ( type == typeof( int ) ) {
		    return "int";
	    }

	    if ( type == typeof( short ) ) {
		    return "short";
	    }

	    if ( type == typeof( ulong ) ) {
		    return "ulong";
	    }

	    if ( type == typeof( uint ) ) {
		    return "uint";
	    }

	    if ( type == typeof( ushort ) ) {
		    return "ushort";
	    }

	    if ( type == typeof( byte ) ) {
		    return "byte";
	    }

	    if ( type == typeof( sbyte ) ) {
		    return "sbyte";
	    }

	    if ( type == typeof( string ) ) {
		    return "string";
	    }

	    if ( type == typeof( void ) ) {
		    return "void";
	    }

		var baseName = new StringBuilder();
		var typeName = includeNamespaces ? type.Namespace + "." + type.Name : type.Name;
	    if ( type.Name.Contains( "`" ) ) {
		    baseName.Append( typeName.Substring( 0, typeName.IndexOf( "`" ) ) );
	    } else {
		    baseName.Append( typeName );
	    }

	    if ( genericParameters.Any() ) {
		    baseName.Append( "<" );
		    for ( var i = 0; i < genericParameters.Length; i++ ) {
			    baseName.Append( GetCSharpTypeName( genericParameters[i], includeNamespaces ) );
			    if ( i + 1 < genericParameters.Length ) {
				    baseName.Append( ", " );
			    }
		    }

		    baseName.Append( ">" );
	    }

	    return baseName.ToString();
    }

	/// <summary>
	/// Converts a <see cref="Type"/> to its C# name. E.g., if you pass in typeof(<see cref="IEnumerable{int}"/>) this
	/// will return "IEnumerable<int>".
	/// This is useful mainly in exception error messages.
	/// </summary>
	/// <param name="type">The type to get a C# description of.</param>
	/// <returns>The string representation of a human-friendly type name</returns>
	public static string GetCSharpTypeName( this Type type, bool includeNamespaces = false ) {
	    if ( type == typeof(long) ) {
		    return "long";
	    }

	    if ( type == typeof( int ) ) {
		    return "int";
	    }

		if ( type == typeof(short) ) {
		    return "short";
	    }

		if ( type == typeof( ulong ) ) {
			return "ulong";
		}

		if ( type == typeof( uint ) ) {
			return "uint";
		}

		if ( type == typeof( ushort ) ) {
			return "ushort";
		}

		if ( type == typeof( byte ) ) {
			return "byte";
		}

		if ( type == typeof( sbyte ) ) {
			return "sbyte";
		}

		if ( type == typeof( string ) ) {
			return "string";
		}

		if ( type == typeof( void ) ) {
			return "void";
		}

		var baseName = new StringBuilder();
		var typeName = includeNamespaces ? type.Namespace + "." + type.Name : type.Name;
		if ( type.Name.Contains( "`" ) ) {
			baseName.Append( typeName.Substring( 0, typeName.IndexOf( "`" ) ) );
		} else {
			baseName.Append( typeName );
		}

		if ( type.GenericTypeArguments.Any() ) {
		    baseName.Append( "<" );
		    for ( var i = 0; i < type.GenericTypeArguments.Length; i++) {
			    baseName.Append( GetCSharpTypeName( type.GenericTypeArguments[i], includeNamespaces ) );
			    if ( i + 1 < type.GenericTypeArguments.Length ) {
				    baseName.Append( ", " );
			    }
		    }

		    baseName.Append(">");
	    }

	    return baseName.ToString();
    }
}