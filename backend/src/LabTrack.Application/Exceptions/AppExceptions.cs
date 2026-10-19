namespace LabTrack.Application.Exceptions;

/// <summary>Recurso não encontrado (HTTP 404).</summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>Conflito com o estado atual, ex.: código duplicado (HTTP 409).</summary>
public class ConflictException(string message) : Exception(message);

/// <summary>Violação de regra de negócio (HTTP 422).</summary>
public class BusinessRuleException(string message) : Exception(message);
