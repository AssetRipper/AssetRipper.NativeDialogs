using Ookii.CommandLine;
using System.ComponentModel;

namespace AssetRipper.NativeDialogs.Example;

[GeneratedParser]
[ParseOptions(IsPosix = true)]
internal sealed partial class Arguments
{
	[CommandLineArgument]
	[Description("Show the open file dialog.")]
	public bool OpenFile { get; set; }

	[CommandLineArgument]
	[Description("Show the open folder dialog.")]
	public bool OpenFolder { get; set; }

	[CommandLineArgument]
	[Description("If opening files or folders, this allows multiple to be chosen.")]
	public bool AllowMultiple { get; set; }

	[CommandLineArgument]
	[Description("Show the save file dialog.")]
	public bool SaveFile { get; set; }

	[CommandLineArgument]
	[Description("Show the message dialog.")]
	public bool Message { get; set; }
}
