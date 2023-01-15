using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.Wordlist;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Models.Identity;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage wordlists.
/// </summary>
[ApiVersion("1.0")]
public class WordlistController : ApiController
{
    private readonly IWordlistRepository _wordlistRepo;
    private readonly IMapper _mapper;

    /// <summary></summary>
    public WordlistController(IWordlistRepository wordlistRepo, IMapper mapper)
    {
        _wordlistRepo = wordlistRepo;
        _mapper = mapper;
    }

    /// <summary>
    /// List all of the available wordlists.
    /// </summary>
    [Guest]
    [HttpGet("all")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<IEnumerable<WordlistDto>>> GetAll()
    {
        var apiUser = HttpContext.GetApiUser();

        var wordlists = apiUser.Role is UserRole.Admin
            ? await _wordlistRepo.GetAll().Include(w => w.Owner).ToListAsync()
            : await _wordlistRepo.GetAll().Include(w => w.Owner)
                .Where(w => w.Owner.Id == apiUser.Id).ToListAsync();

        return Ok(_mapper.Map<IEnumerable<WordlistDto>>(wordlists));
    }
}
