using SysLog.Domine.Interfaces.Repositories;
using SysLog.Repository.Model;
using SysLog.Service.Interfaces.Services;
using SysLog.Service.Mappers;
using SysLog.Shared.ModelDto;
using System;

namespace SysLog.Service.Services;

public class BackupFileService(IBackupFileRepository repository) : Service<BackupFileDto,BackupFile>(repository),IBackupFileService
{
    public int GetLastBackupFileDayTime()
    {
        return repository.GetLastBackupFileDayTime();
    }

    public async Task<BackupFileDto?> FindByDateAsync(string day)
    {
        var entity = await repository.FindByDateAsync(day);
        return MapperBackup.MapToDto(entity);
    }

    public async Task<List<BackupFileDto>> FindAllByDateAsync(string dayPrefix)
    {
        var entities = await repository.FindAllByDateAsync(dayPrefix);
        return entities.Select(MapperBackup.MapToDto).ToList();
    }

    public async Task<List<string>> GetAvailableDaysAsync()
    {
        return await repository.GetAvailableDaysAsync();
    }
}