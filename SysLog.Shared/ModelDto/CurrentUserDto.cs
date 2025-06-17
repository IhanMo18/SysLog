namespace SysLog.Shared.ModelDto;

public sealed class CurrentUserDto
{
    public bool IsAuthenticated { get; set; }
    public IEnumerable<ClaimDto> Claims { get; set; } = Enumerable.Empty<ClaimDto>();
}

public sealed class ClaimDto
{
    public string Type  { get; set; } = default!;
    public string Value { get; set; } = default!;
}

