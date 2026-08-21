using System.ComponentModel;
using DSharpPlus.Commands;
using Hammer.Services;

namespace Hammer.Commands.Notes;

/// <summary>
///     Represents a class which implements the <c>note</c> command.
/// </summary>
[Command("note")]
[Description("Manages member notes.")]
internal sealed partial class NoteCommand
{
    private readonly ILogger<NoteCommand> _logger;
    private readonly ConfigurationService _configurationService;
    private readonly MemberNoteService _noteService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NoteCommand" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configurationService">The configuration service.</param>
    /// <param name="noteService">The note service.</param>
    public NoteCommand(ILogger<NoteCommand> logger, ConfigurationService configurationService, MemberNoteService noteService)
    {
        _logger = logger;
        _configurationService = configurationService;
        _noteService = noteService;
    }
}
