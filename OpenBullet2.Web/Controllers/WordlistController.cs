using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Web.Auth;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.Wordlist;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Interfaces;
using OpenBullet2.Web.Models.Identity;
using OpenBullet2.Web.Services;
using RuriLib.Extensions;
using RuriLib.Functions.Files;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage wordlists.
/// </summary>
/// <remarks></remarks>
[TypeFilter<GuestFilter>]
[ApiVersion("1.0")]
public class WordlistController(IWordlistRepository wordlistRepo,
    IGuestRepository guestRepo, IObjectMapper mapper,
    ILogger<WordlistController> logger,
    UserDataDirectoryProvider userDataDirectory) : ApiController
{
    private readonly IGuestRepository _guestRepo = guestRepo;
    private readonly ILogger<WordlistController> _logger = logger;
    private readonly IObjectMapper _mapper = mapper;
    private readonly UserDataDirectoryProvider _userDataDirectory = userDataDirectory;
    private readonly IWordlistRepository _wordlistRepo = wordlistRepo;
    private static readonly string[] ScriptExtensions = [".bat", ".ps1", ".sh"];

    /// <summary>
    /// Get a wordlist by id.
    /// </summary>
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<WordlistDto>> Get(int id, CancellationToken cancellationToken)
    {
        var wordlist = await GetEntityAsync(id, cancellationToken);
        EnsureOwnership(wordlist);
        return _mapper.Map<WordlistDto>(wordlist);
    }

    /// <summary>
    /// List all the available wordlists.
    /// </summary>
    [HttpGet("all")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<IEnumerable<WordlistDto>>> GetAll(CancellationToken cancellationToken)
    {
        var apiUser = HttpContext.GetApiUser();

        var entities = apiUser.Role is UserRole.Admin
            ? await _wordlistRepo.GetAll().Include(w => w.Owner).ToListAsync(cancellationToken)
            : await _wordlistRepo.GetAll().Include(w => w.Owner)
                .Where(w => w.Owner != null && w.Owner.Id == apiUser.Id).ToListAsync(cancellationToken);

        return Ok(_mapper.Map<IEnumerable<WordlistDto>>(entities));
    }

    /// <summary>
    /// Get a preview of the content of the wordlist.
    /// </summary>
    /// <param name="id">The id of the wordlist</param>
    /// <param name="lineCount">How many lines to preview</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [HttpGet("preview")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<WordlistPreviewDto>> GetPreview(int id,
        int lineCount = 10, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityAsync(id, cancellationToken);

        EnsureOwnership(entity);

        if (!System.IO.File.Exists(entity.FileName))
        {
            var fileName = entity.FileName ?? string.Empty;

            _logger.LogWarning("The wordlist with id {Id} references a file that was moved or deleted from {FileName}",
                id, fileName);

            throw new ResourceNotFoundException(
                ErrorCode.FileNotFound,
                Path.GetFileName(fileName), fileName);
        }

        var wordlistPath = entity.FileName ?? string.Empty;
        var firstLines = System.IO.File.ReadLines(wordlistPath)
            .Take(lineCount).ToArray();

        var size = new FileInfo(wordlistPath).Length;

        return new WordlistPreviewDto { FirstLines = firstLines, SizeInBytes = size };
    }

    /// <summary>
    /// Create a wordlist by referencing a file on the server. Use this
    /// endpoint once you already uploaded the file to the server.
    /// </summary>
    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<WordlistDto>> Create(CreateWordlistDto dto,
        [FromServices] IValidator<CreateWordlistDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(dto, cancellationToken);

        var apiUser = HttpContext.GetApiUser();

        // If the user is a guest, make sure they are not accessing
        // anything outside the UserData folder.
        if (apiUser.Role is UserRole.Guest && !dto.FilePath.IsSubPathOf(_userDataDirectory.RootPath))
        {
            _logger.LogWarning(
                "Guest user {Username} tried to access a file outside of the allowed directory while creating a wordlist at {FilePath}",
                apiUser.Username, dto.FilePath);

            throw new ForbiddenException(ErrorCode.FileOutsideAllowedPath,
                $"Guest users cannot access files outside of the {_userDataDirectory.RootPath} folder");
        }

        // Make sure the file exists
        var path = dto.FilePath.Replace('\\', '/');

        if (!System.IO.File.Exists(path))
        {
            _logger.LogWarning("Tried to create a wordlist for the file {FilePath} which does not exist",
                dto.FilePath);

            throw new ResourceNotFoundException(
                ErrorCode.FileNotFound,
                Path.GetFileName(dto.FilePath), dto.FilePath);
        }

        var entity = new WordlistEntity
        {
            Name = dto.Name,
            FileName = path,
            Purpose = dto.Purpose,
            Total = System.IO.File.ReadLines(path).Count(),
            Type = dto.WordlistType
        };

        // If the user is a guest, we also have to mark them as the
        // owner of the wordlist, for access control purposes.
        if (apiUser.Role is UserRole.Guest)
        {
            entity.Owner = await _guestRepo.GetAsync(apiUser.Id, cancellationToken);
        }

        await _wordlistRepo.AddAsync(entity, cancellationToken);

        _logger.LogInformation("Created a new wordlist with id {Id} for file {FileName}",
            entity.Id, entity.FileName);

        return _mapper.Map<WordlistDto>(entity);
    }

    /// <summary>
    /// Upload a file to the user data Wordlists folder. This can then be
    /// used to create a wordlist. Returns the relative file path.
    /// </summary>
    [TypeFilter<GuestFilter>]
    [HttpPost("upload")]
    [MapToApiVersion("1.0")]
    [RequestSizeLimit(long.MaxValue)]
    public async Task<ActionResult<WordlistFileDto>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        var uploadedFileName = Path.GetFileName(file.FileName);

        if (string.IsNullOrWhiteSpace(uploadedFileName))
        {
            throw new BadRequestException(
                ErrorCode.ValidationError, "The uploaded file name is invalid");
        }

        if (ScriptExtensions.Contains(Path.GetExtension(uploadedFileName),
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ForbiddenException(
                ErrorCode.ScriptFileNotAllowed,
                "Uploading script files as wordlists is not allowed");
        }

        var safeFileName = FileUtils.ReplaceInvalidFileNameChars(uploadedFileName);
        var wordlistsDir = _userDataDirectory.GetPath("Wordlists");
        Directory.CreateDirectory(wordlistsDir);

        var path = FileUtils.GetFirstAvailableFileName(
            Path.Combine(wordlistsDir, safeFileName));

        await using var fileStream = System.IO.File.OpenWrite(path);
        await file.CopyToAsync(fileStream, cancellationToken);

        _logger.LogInformation("Uploaded a wordlist file at {Path}", path);

        return new WordlistFileDto { FilePath = path };
    }

    /// <summary>
    /// Delete a wordlist.
    /// </summary>
    /// <param name="id">The id of the wordlist to delete</param>
    /// <param name="alsoDeleteFile">Whether to also delete the file from disk</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [TypeFilter<GuestFilter>]
    [HttpDelete]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> Delete(int id, bool alsoDeleteFile, CancellationToken cancellationToken)
    {
        var entity = await GetEntityAsync(id, cancellationToken);

        EnsureOwnership(entity);

        await _wordlistRepo.DeleteAsync(entity, alsoDeleteFile, cancellationToken);

        _logger.LogInformation("Deleted the wordlist with id {Id}", id);

        return Ok();
    }

    /// <summary>
    /// Delete all the wordlists that reference a file that was moved or deleted.
    /// </summary>
    [TypeFilter<GuestFilter>]
    [HttpDelete("not-found")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<AffectedEntriesDto>> DeleteNotFound(CancellationToken cancellationToken)
    {
        var apiUser = HttpContext.GetApiUser();

        var entities = apiUser.Role is UserRole.Admin
            ? await _wordlistRepo.GetAll().Include(w => w.Owner).ToListAsync(cancellationToken)
            : await _wordlistRepo.GetAll().Include(w => w.Owner)
                .Where(w => w.Owner != null && w.Owner.Id == apiUser.Id).ToListAsync(cancellationToken);

        var deletedCount = 0;

        foreach (var entity in entities.Where(entity => !System.IO.File.Exists(entity.FileName)))
        {
            deletedCount++;
            await _wordlistRepo.DeleteAsync(entity, cancellationToken: cancellationToken);
        }

        _logger.LogInformation(
            "Deleted {DeletedCount} wordlists that referenced files that were moved or deleted",
            deletedCount);

        return new AffectedEntriesDto { Count = deletedCount };
    }

    /// <summary>
    /// Update the info of a wordlist.
    /// </summary>
    [TypeFilter<GuestFilter>]
    [HttpPatch("info")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<WordlistDto>> UpdateInfo(
        UpdateWordlistInfoDto dto,
        [FromServices] IValidator<UpdateWordlistInfoDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(dto, cancellationToken);

        var entity = await GetEntityAsync(dto.Id, cancellationToken);

        EnsureOwnership(entity);

        _mapper.Map(dto, entity);
        await _wordlistRepo.UpdateAsync(entity, cancellationToken);

        _logger.LogInformation("Updated the information of the wordlist with id {Id}",
            entity.Id);

        return _mapper.Map<WordlistDto>(entity);
    }

    private async Task<WordlistEntity> GetEntityAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _wordlistRepo.GetAsync(id, cancellationToken) ?? throw new EntryNotFoundException(ErrorCode.WordlistNotFound,
                id, nameof(IWordlistRepository));
        return entity;
    }

    private void EnsureOwnership(WordlistEntity entity)
    {
        var apiUser = HttpContext.GetApiUser();

        // If the user is trying to access a wordlist that is not owned
        // by them, throw a not found exception.
        if (apiUser.Role is UserRole.Guest && apiUser.Id != entity.Owner?.Id)
        {
            _logger.LogWarning("Guest user {Username} tried to access a wordlist not owned by them",
                apiUser.Username);

            throw new EntryNotFoundException(ErrorCode.WordlistNotFound,
                entity.Id, nameof(IWordlistRepository));
        }
    }
}
