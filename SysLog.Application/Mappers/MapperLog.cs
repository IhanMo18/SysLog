using SysLog.Repository.Model;
using SysLog.Shared.ModelDto;

namespace SysLog.Service.Mappers;

public static class MapperLog
{
    public static LogDto MapToLogDto(Log log)
    {
        return new LogDto()
        {
            Action = MapToActionDto(log),
            Interface = MapToInterfaceDto(log),
            Protocol = MapToProtocolDto(log),
            LogType = MapToLogTypeDto(log),
            IpDestiny = log.IpDestiny,
            IpOut = log.IpOut,
            DateTime = log.DateTime
        };
    }

    private static ActionDto? MapToActionDto(Log log)
    {
        if (log.Action == null)
            return null;
        
        return new ActionDto()
        {
            AcctionName = log.Action.AcctionName
        };
    }   
    private static InterfaceDto? MapToInterfaceDto(Log log)
    {
        if (log.Interface == null)
            return null;
        
        return new InterfaceDto()
        {
            Name = log.Interface.Name
        };
    }   
    
    private static ProtocolDto? MapToProtocolDto(Log log)
    {
        if (log.Protocol == null)
            return null;
        return new ProtocolDto()
        {
            Name = log.Protocol.Name
        };
    }  
    private static LogTypeDto? MapToLogTypeDto(Log log)
    {
        if (log.LogType == null)
            return null;
        return new LogTypeDto()
        {
            TypeName= log.LogType.TypeName,
            Signature = MapToSignatureDto(log.LogType.Signature),  
        };
    }

    private static SignatureDto? MapToSignatureDto(Signature? signature)
    {
        if (signature == null)
            return null;
        return new SignatureDto()
        {
            Message = signature.Message
        };
    }
}