namespace OrdemCerta.Domain.Admin.DTOs;

public record AdminTokenOutput(string Token, DateTime ExpiresAt);
