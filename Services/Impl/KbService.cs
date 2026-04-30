using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.Info;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.Kb;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

public class KbService : IKbService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<KbService> _logger;

    private static readonly string[] AllowedExts =
        { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
          ".txt", ".png", ".jpg", ".jpeg", ".zip", ".rar" };
    private const long MaxFileSize = 50 * 1024 * 1024;

    public KbService(IUnitOfWork uow, ILogger<KbService> logger)
    { _uow = uow; _logger = logger; }

    public async Task<PagedResult<KbFileDto>> GetPagedAsync(KbQueryDto query, bool adminView = false)
    {
        var q = _uow.KbFiles.Query()
            .Include(f => f.Category)
            .AsQueryable();

        if (!adminView)
            q = q.Where(f => f.Status == 1);

        if (query.CategoryId.HasValue)
            q = q.Where(f => f.CategoryId == query.CategoryId.Value);
        if (query.Status.HasValue)
            q = q.Where(f => f.Status == query.Status.Value);
        if (!string.IsNullOrWhiteSpace(query.Keyword))
            q = q.Where(f => f.FileName.Contains(query.Keyword)
                           || (f.Description != null && f.Description.Contains(query.Keyword)));

        var total = await q.CountAsync();
        var list = await q.OrderByDescending(f => f.IsPinned)
                          .ThenByDescending(f => f.CreatedAt)
                          .Skip((query.Page - 1) * query.Size).Take(query.Size)
                          .ToListAsync();

        return new PagedResult<KbFileDto>
        {
            Items = list.Select(f => MapToDto(f)).ToList(),
            Total = total,
            Page = query.Page,
            PageSize = query.Size,
        };
    }

    public async Task<KbFileDto?> GetDetailAsync(long id)
    {
        var f = await _uow.KbFiles.Query()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);
        return f == null ? null : MapToDto(f);
    }

    public async Task<long> UploadAsync(KbUploadDto dto, string operBy)
    {
        if (dto.File.Length > MaxFileSize)
            throw new BusinessException("文件大小不能超过50MB");

        var ext = Path.GetExtension(dto.File.FileName).ToLower();
        if (!AllowedExts.Contains(ext))
            throw new BusinessException($"不支持的文件类型 {ext}");

        var category = await _uow.KbCategories.GetByIdAsync(dto.CategoryId)
            ?? throw new BusinessException("分类不存在");

        var saveDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "kb",
                                   dto.CategoryId.ToString());
        Directory.CreateDirectory(saveDir);
        var saveName = $"{DateTime.Now:yyyyMMdd}_{Guid.NewGuid():N}{ext}";
        var savePath = Path.Combine(saveDir, saveName);

        try
        {
            using (var fs = new FileStream(savePath, FileMode.Create))
                await dto.File.CopyToAsync(fs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件保存失败：{Path}", savePath);
            throw new BusinessException("文件保存失败，请检查磁盘空间");
        }

        var kbFile = new KbFile
        {
            CategoryId   = dto.CategoryId,
            FileName     = string.IsNullOrWhiteSpace(dto.DisplayName)
                           ? Path.GetFileNameWithoutExtension(dto.File.FileName) : dto.DisplayName,
            OriginalName = dto.File.FileName,
            FilePath     = savePath,
            FileSize     = dto.File.Length,
            FileExt      = ext.TrimStart('.'),
            Description  = dto.Description,
            Version      = dto.Version,
            IsPinned     = dto.IsPinned,
            Status       = 1,
            CreatedBy    = operBy,
        };
        await _uow.KbFiles.AddAsync(kbFile);
        await _uow.SaveChangesAsync();
        _logger.LogInformation("上传知识库文件：[{Cat}] {Name} by {User}", category.Name, kbFile.FileName, operBy);
        return kbFile.Id;
    }

    public async Task UpdateAsync(KbUpdateDto dto, string operBy)
    {
        var f = await _uow.KbFiles.GetByIdAsync(dto.Id)
            ?? throw new NotFoundException("文件不存在");

        var category = await _uow.KbCategories.GetByIdAsync(dto.CategoryId)
            ?? throw new BusinessException("分类不存在");

        f.FileName    = dto.FileName;
        f.CategoryId  = dto.CategoryId;
        f.Description = dto.Description;
        f.Version     = dto.Version;
        f.IsPinned    = dto.IsPinned;
        f.Status      = dto.Status;
        f.UpdatedBy   = operBy;
        _uow.KbFiles.Update(f);
        await _uow.SaveChangesAsync();
    }

    public async Task TogglePinAsync(long id, string operBy)
    {
        var f = await _uow.KbFiles.GetByIdAsync(id)
            ?? throw new NotFoundException("文件不存在");
        f.IsPinned  = !f.IsPinned;
        f.UpdatedBy = operBy;
        _uow.KbFiles.Update(f);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id, string operBy)
    {
        var f = await _uow.KbFiles.GetByIdAsync(id)
            ?? throw new NotFoundException("文件不存在");
        f.UpdatedBy = operBy;
        _uow.KbFiles.SoftDelete(f);
        await _uow.SaveChangesAsync();
    }

    public async Task<(string path, string name, string mime, long size)?> GetDownloadInfoAsync(long id)
    {
        var f = await _uow.KbFiles.GetByIdAsync(id);
        if (f == null || !File.Exists(f.FilePath))
            return null;

        f.DownloadCount++;
        _uow.KbFiles.Update(f);
        await _uow.SaveChangesAsync();

        return (f.FilePath, f.OriginalName, GetMimeType(f.FileExt ?? "bin"), f.FileSize);
    }

    public async Task<List<KbCategoryDto>> GetCategoriesAsync()
    {
        var cats = await _uow.KbCategories.Query()
            .Where(c => c.Status == 1)
            .OrderBy(c => c.Sort)
            .ToListAsync();

        var fileCounts = await _uow.KbFiles.Query()
            .Where(f => f.Status == 1)
            .GroupBy(f => f.CategoryId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync();
        var countDict = fileCounts.ToDictionary(x => x.Key, x => x.Count);

        return cats.Select(c => new KbCategoryDto
        {
            Id          = c.Id,
            Name        = c.Name,
            Icon        = c.Icon,
            Description = c.Description,
            Sort        = c.Sort,
            Status      = c.Status,
            FileCount   = countDict.GetValueOrDefault(c.Id),
        }).ToList();
    }

    private static KbFileDto MapToDto(KbFile f) => new()
    {
        Id            = f.Id,
        CategoryId    = f.CategoryId,
        CategoryName  = f.Category?.Name ?? "",
        FileName      = f.FileName,
        OriginalName  = f.OriginalName,
        FileSize      = f.FileSize,
        FileExt       = f.FileExt,
        Description   = f.Description,
        Version       = f.Version,
        DownloadCount = f.DownloadCount,
        IsPinned      = f.IsPinned,
        Status        = f.Status,
        CreatedBy     = f.CreatedBy,
        CreatedAt     = f.CreatedAt,
    };

    internal static string GetMimeType(string ext) => ext.ToLower() switch
    {
        "pdf"  => "application/pdf",
        "doc"  => "application/msword",
        "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "xls"  => "application/vnd.ms-excel",
        "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "ppt"  => "application/vnd.ms-powerpoint",
        "pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "txt"  => "text/plain",
        "jpg" or "jpeg" => "image/jpeg",
        "png"  => "image/png",
        "zip"  => "application/zip",
        "rar"  => "application/x-rar-compressed",
        _      => "application/octet-stream",
    };
}
