namespace SysLog.Shared.ModelDto;

public sealed class CurrentUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAuthenticated { get; set; }
    public IEnumerable<ClaimDto> Claims { get; set; } = Enumerable.Empty<ClaimDto>();
}

public sealed class ClaimDto
{
    public string Type  { get; set; } = default!;
    public string Value { get; set; } = default!;
}

