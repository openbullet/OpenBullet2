using AngleSharp.Dom;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.Wordlist;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Models.Identity;
using RuriLib.Extensions;
using RuriLib.Functions.Files;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage wordlists.
/// </summary>
[Guest]
[ApiVersion("1.0")]
public class WordlistController : ApiController
{
    private readonly IWordlistRepository _wordlistRepo;
    private readonly IGuestRepository _guestRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<WordlistController> _logger;
    private readonly string _baseDir = "UserData";

    /// <summary></summary>
    public WordlistController(IWordlistRepository wordlistRepo,
        IGuestRepository guestRepo, IMapper mapper,
        ILogger<WordlistController> logger)
    {
        _wordlistRepo = wordlistRepo;
        _guestRepo = guestRepo;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// List all of the available wordlists.
    /// </summary>
    [HttpGet("all")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<IEnumerable<WordlistDto>>> GetAll()
    {
        var apiUser = HttpContext.GetApiUser();

        var entities = apiUser.Role is UserRole.Admin
            ? await _wordlistRepo.GetAll().Include(w => w.Owner).ToListAsync()
            : await _wordlistRepo.GetAll().Include(w => w.Owner)
                .Where(w => w.Owner.Id == apiUser.Id).ToListAsync();

        return Ok(_mapper.Map<IEnumerable<WordlistDto>>(entities));
    }

    /// <summary>
    /// Get a preview of the content of the wordlist.
    /// </summary>
    /// <param name="id">The id of the wordlist</param>
    /// <param name="lineCount">How many lines to preview</param>
    [HttpGet("preview")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<WordlistPreviewDto>> GetPreview(int id,
        int lineCount = 10)
    {
        var entity = await GetEntityAsync(id);

        EnsureOwnership(entity);

        if (!System.IO.File.Exists(entity.FileName))
        {
            _logger.LogWarning($"The wordlist with id {id} references a file that was moved or deleted from {entity.FileName}");

            throw new ResourceNotFoundException(
                ErrorCode.FILE_NOT_FOUND,
                Path.GetFileName(entity.FileName), entity.FileName);
        }

        var firstLines = System.IO.File.ReadLines(entity.FileName)
            .Take(lineCount).ToArray();

        var size = new FileInfo(entity.FileName).Length;

        return new WordlistPreviewDto
        {
            FirstLines = firstLines,
            SizeInBytes = size
        };
    }

    /// <summary>
    /// Create a wordlist by referencing a file on the server. Use this
    /// endpoint once you already uploaded the file to the server.
    /// </summary>
    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<WordlistDto>> Create(CreateWordlistDto dto)
    {
        var apiUser = HttpContext.GetApiUser();

        // If the user is a guest, make sure they are not accessing
        // anything outside of the UserData folder.
        if (apiUser.Role is UserRole.Guest)
        {
            if (!dto.FilePath.IsSubPathOf(_baseDir))
            {
                _logger.LogWarning($"Guest user {apiUser.Username} tried to access a file outside of the allowed directory while creating a wordlist at {dto.FilePath}");

                throw new UnauthorizedAccessException(
                    $"Guest users cannot access files outside of the {_baseDir} folder");
            }
        }

        // Make sure the file exists
        if (!System.IO.File.Exists(dto.FilePath))
        {
            _logger.LogWarning($"Tried to create a wordlist for the file {dto.FilePath} which does not exist");

            throw new ResourceNotFoundException(
                ErrorCode.FILE_NOT_FOUND,
                Path.GetFileName(dto.FilePath), dto.FilePath);
        }

        var entity = _mapper.Map<WordlistEntity>(dto);

        // If the user is a guest, we also have to mark them as the
        // owner of the wordlist, for access control purposes.
        if (apiUser.Role is UserRole.Guest)
        {
            var owner = await _guestRepo.Get(apiUser.Id);
            entity.Owner = owner;
        }

        await _wordlistRepo.Add(entity);

        _logger.LogInformation($"Created a new wordlist with id {entity.Id} for file {entity.FileName}");

        return _mapper.Map<WordlistDto>(entity);
    }

    /// <summary>
    /// Upload a file to the "UserData/Wordlists" folder. This can then be
    /// used to create a wordlist. Returns the relative file path.
    /// </summary>
    [Guest]
    [HttpPost("upload")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<WordlistFileDto>> Upload(IFormFile file)
    {
        var path = FileUtils.GetFirstAvailableFileName(
            Path.Combine(_baseDir, file.FileName));

        using var fileStream = System.IO.File.OpenWrite(path);
        await file.CopyToAsync(fileStream);

        _logger.LogInformation($"Uploaded a wordlist file at {path}");

        return new WordlistFileDto { FilePath = path };
    }

    /// <summary>
    /// Delete a wordlist.
    /// </summary>
    /// <param name="id">The id of the wordlist to delete</param>
    /// <param name="alsoDeleteFile">Whether to also delete the file from disk</param>
    [Guest]
    [HttpDelete]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> Delete(int id, bool alsoDeleteFile)
    {
        var entity = await GetEntityAsync(id);

        EnsureOwnership(entity);

        await _wordlistRepo.Delete(entity, alsoDeleteFile);

        return Ok();
    }

    /// <summary>
    /// Delete all the wordlists that reference a file that was moved or deleted.
    /// </summary>
    [Guest]
    [HttpDelete("not-found")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<AffectedEntriesDto>> DeleteNotFound()
    {
        var apiUser = HttpContext.GetApiUser();

        var entities = apiUser.Role is UserRole.Admin
            ? await _wordlistRepo.GetAll().Include(w => w.Owner).ToListAsync()
            : await _wordlistRepo.GetAll().Include(w => w.Owner)
                .Where(w => w.Owner.Id == apiUser.Id).ToListAsync();

        var deletedCount = 0;

        foreach (var entity in entities)
        {
            if (!System.IO.File.Exists(entity.FileName))
            {
                deletedCount++;
                await _wordlistRepo.Delete(entity, deleteFile: false);
            }
        }

        return new AffectedEntriesDto
        {
            Count = deletedCount
        };
    }

    /// <summary>
    /// Update the info of a wordlist.
    /// </summary>
    [Guest]
    [HttpPatch("info")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<WordlistDto>> UpdateInfo(
        UpdateWordlistInfoDto dto)
    {
        var entity = await GetEntityAsync(dto.Id);

        EnsureOwnership(entity);

        _mapper.Map(dto, entity);
        await _wordlistRepo.Update(entity);

        _logger.LogInformation($"Updated the information of the wordlist with id {entity.Id}");

        return _mapper.Map<WordlistDto>(entity);
    }

    private async Task<WordlistEntity> GetEntityAsync(int id)
    {
        var entity = await _wordlistRepo.Get(id);

        if (entity is null)
        {
            throw new EntryNotFoundException(ErrorCode.WORDLIST_NOT_FOUND,
                id, nameof(IWordlistRepository));
        }

        return entity;
    }

    private void EnsureOwnership(WordlistEntity entity)
    {
        var apiUser = HttpContext.GetApiUser();

        // If the user is trying to access a wordlist that is not owned
        // by them, throw a not found exception.
        if (apiUser.Role is UserRole.Guest && apiUser.Id != entity.Owner?.Id)
        {
            _logger.LogWarning($"Guest user {apiUser.Username} tried to access a wordlist not owned by them");

            throw new EntryNotFoundException(ErrorCode.WORDLIST_NOT_FOUND,
                entity.Id, nameof(IWordlistRepository));
        }
    }
}
