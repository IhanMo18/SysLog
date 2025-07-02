using SysLog.Repository.Model;
using SysLog.Shared.ModelDto;

namespace SysLog.Service.Mappers;

public static class MapperBackup
{

    public static BackupFileDto MapToDto(this BackupFile backupFile)
    {
        return new BackupFileDto
        {
            FileName = backupFile.FileName,
            PathFile = backupFile.PathFile
        };
    }


    public static BackupFile MapToEntity(this BackupFileDto backupFileDto)
    {
        return new BackupFile()
        {
            FileName = backupFileDto.FileName,
            PathFile = backupFileDto.PathFile
        };
    }
}