namespace SysLog.Shared.ModelDto;

public record CurrentUserDto(
    string UserId,
    string Username,
    string Email,
    bool IsAuthenticated);