using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TombLib.Scripting.ClassicScript.StringTables;

/// <summary>
/// The parsed logical sections of a ClassicScript string-table source.
/// </summary>
public sealed class ClassicScriptStringTable
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptStringTable"/> class.
	/// </summary>
	/// <param name="sections">The sections in source order.</param>
	public ClassicScriptStringTable(IEnumerable<ClassicScriptStringTableSection> sections)
	{
		ArgumentNullException.ThrowIfNull(sections);
		Sections = new ReadOnlyCollection<ClassicScriptStringTableSection>(sections.ToArray());
	}

	/// <summary>
	/// Gets the sections in source order.
	/// </summary>
	public IReadOnlyList<ClassicScriptStringTableSection> Sections { get; }
}

/// <summary>
/// A parsed section in a ClassicScript string table.
/// </summary>
public sealed class ClassicScriptStringTableSection
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptStringTableSection"/> class.
	/// </summary>
	/// <param name="name">The section name without surrounding brackets.</param>
	/// <param name="isExtraNg"><see langword="true"/> when rows use the ExtraNG ID format.</param>
	/// <param name="rows">The rows in source order.</param>
	public ClassicScriptStringTableSection(
		string name,
		bool isExtraNg,
		IEnumerable<ClassicScriptStringTableRow> rows)
	{
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(rows);

		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("A section name is required.", nameof(name));

		Name = name;
		IsExtraNg = isExtraNg;
		Rows = new ReadOnlyCollection<ClassicScriptStringTableRow>(rows.ToArray());
	}

	/// <summary>
	/// Gets the section name without surrounding brackets.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Gets a value indicating whether rows contain explicit ExtraNG IDs.
	/// </summary>
	public bool IsExtraNg { get; }

	/// <summary>
	/// Gets the rows in source order.
	/// </summary>
	public IReadOnlyList<ClassicScriptStringTableRow> Rows { get; }
}

/// <summary>
/// A parsed string-table row.
/// </summary>
public sealed class ClassicScriptStringTableRow
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptStringTableRow"/> class.
	/// </summary>
	/// <param name="id">The explicit ExtraNG ID, or <see langword="null"/> for a normal row.</param>
	/// <param name="value">The decoded row value.</param>
	public ClassicScriptStringTableRow(int? id, string value)
	{
		ArgumentNullException.ThrowIfNull(value);

		Id = id;
		Value = value;
	}

	/// <summary>
	/// Gets the explicit ExtraNG ID, or <see langword="null"/> for a normal row.
	/// </summary>
	public int? Id { get; }

	/// <summary>
	/// Gets the decoded row value.
	/// </summary>
	public string Value { get; }
}

/// <summary>
/// Identifies the line ending emitted by the ClassicScript string-table writer.
/// </summary>
public enum ClassicScriptStringTableNewlineStyle
{
	/// <summary>
	/// Carriage return followed by line feed.
	/// </summary>
	CrLf,

	/// <summary>
	/// Line feed.
	/// </summary>
	Lf,

	/// <summary>
	/// Carriage return.
	/// </summary>
	Cr
}

/// <summary>
/// Supplies explicit newline behavior while parsing escaped embedded newlines.
/// </summary>
public sealed class ClassicScriptStringTableParseOptions
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptStringTableParseOptions"/> class.
	/// </summary>
	/// <param name="parsedNewline">The newline represented by a <c>\n</c> token.</param>
	public ClassicScriptStringTableParseOptions(string parsedNewline = "\n")
	{
		if (parsedNewline is not ("\r\n" or "\n" or "\r"))
			throw new ArgumentException("The parsed newline must be CRLF, LF, or CR.", nameof(parsedNewline));

		ParsedNewline = parsedNewline;
	}

	/// <summary>
	/// Gets the newline represented by a <c>\n</c> token in a row.
	/// </summary>
	public string ParsedNewline { get; }
}

/// <summary>
/// Identifies the explicit generated header metadata written before sections.
/// </summary>
public sealed class ClassicScriptStringTableHeader
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptStringTableHeader"/> class.
	/// </summary>
	/// <param name="generatorName">The name written after <c>using</c>.</param>
	/// <param name="generatorVersion">The version written after the generator name.</param>
	public ClassicScriptStringTableHeader(string generatorName, string generatorVersion)
	{
		ArgumentNullException.ThrowIfNull(generatorName);
		ArgumentNullException.ThrowIfNull(generatorVersion);

		if (string.IsNullOrWhiteSpace(generatorName))
			throw new ArgumentException("A generator name is required.", nameof(generatorName));

		if (string.IsNullOrWhiteSpace(generatorVersion))
			throw new ArgumentException("A generator version is required.", nameof(generatorVersion));

		GeneratorName = generatorName;
		GeneratorVersion = generatorVersion;
	}

	/// <summary>
	/// Gets the generator name.
	/// </summary>
	public string GeneratorName { get; }

	/// <summary>
	/// Gets the generator version.
	/// </summary>
	public string GeneratorVersion { get; }
}

/// <summary>
/// Supplies explicit output formatting for normalized string-table serialization.
/// </summary>
public sealed class ClassicScriptStringTableWriteOptions
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptStringTableWriteOptions"/> class.
	/// </summary>
	/// <param name="newlineStyle">The line ending to emit.</param>
	/// <param name="generatedHeader">Optional generated-header metadata.</param>
	public ClassicScriptStringTableWriteOptions(
		ClassicScriptStringTableNewlineStyle newlineStyle = ClassicScriptStringTableNewlineStyle.Lf,
		ClassicScriptStringTableHeader? generatedHeader = null)
	{
		NewlineStyle = newlineStyle;
		GeneratedHeader = generatedHeader;
	}

	/// <summary>
	/// Gets the line ending to emit.
	/// </summary>
	public ClassicScriptStringTableNewlineStyle NewlineStyle { get; }

	/// <summary>
	/// Gets optional generated-header metadata.
	/// </summary>
	public ClassicScriptStringTableHeader? GeneratedHeader { get; }
}

/// <summary>
/// Identifies a parser diagnostic.
/// </summary>
public enum ClassicScriptStringTableDiagnosticCode
{
	/// <summary>
	/// A section header is malformed or empty.
	/// </summary>
	MalformedSectionHeader,

	/// <summary>
	/// An ExtraNG row does not contain the required integer and colon.
	/// </summary>
	MalformedExtraNgRow,

	/// <summary>
	/// An ExtraNG row ID is not a valid integer.
	/// </summary>
	InvalidExtraNgId,

	/// <summary>
	/// A section name occurs more than once.
	/// </summary>
	DuplicateSectionName,

	/// <summary>
	/// Non-comment text occurs before the first section.
	/// </summary>
	UnexpectedPreambleText
}

/// <summary>
/// Describes one typed string-table parse diagnostic.
/// </summary>
public sealed class ClassicScriptStringTableDiagnostic
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptStringTableDiagnostic"/> class.
	/// </summary>
	/// <param name="code">The diagnostic code.</param>
	/// <param name="line">The one-based source line.</param>
	/// <param name="column">The one-based source column.</param>
	/// <param name="message">The diagnostic message.</param>
	public ClassicScriptStringTableDiagnostic(
		ClassicScriptStringTableDiagnosticCode code,
		int line,
		int column,
		string message)
	{
		ArgumentNullException.ThrowIfNull(message);

		Code = code;
		Line = line;
		Column = column;
		Message = message;
	}

	/// <summary>
	/// Gets the diagnostic code.
	/// </summary>
	public ClassicScriptStringTableDiagnosticCode Code { get; }

	/// <summary>
	/// Gets the one-based source line.
	/// </summary>
	public int Line { get; }

	/// <summary>
	/// Gets the one-based source column.
	/// </summary>
	public int Column { get; }

	/// <summary>
	/// Gets the diagnostic message.
	/// </summary>
	public string Message { get; }
}

/// <summary>
/// Contains the result of parsing a ClassicScript string-table source.
/// </summary>
public sealed class ClassicScriptStringTableParseResult
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptStringTableParseResult"/> class.
	/// </summary>
	/// <param name="model">The parsed model, or <see langword="null"/> when diagnostics exist.</param>
	/// <param name="diagnostics">The diagnostics produced by parsing.</param>
	public ClassicScriptStringTableParseResult(
		ClassicScriptStringTable? model,
		IEnumerable<ClassicScriptStringTableDiagnostic> diagnostics)
	{
		ArgumentNullException.ThrowIfNull(diagnostics);

		Model = model;
		Diagnostics = new ReadOnlyCollection<ClassicScriptStringTableDiagnostic>(diagnostics.ToArray());
	}

	/// <summary>
	/// Gets a value indicating whether parsing produced a publishable model.
	/// </summary>
	public bool IsSuccess => Model is not null && Diagnostics.Count == 0;

	/// <summary>
	/// Gets the parsed model, or <see langword="null"/> when parsing failed.
	/// </summary>
	public ClassicScriptStringTable? Model { get; }

	/// <summary>
	/// Gets the typed diagnostics in source order.
	/// </summary>
	public IReadOnlyList<ClassicScriptStringTableDiagnostic> Diagnostics { get; }
}
