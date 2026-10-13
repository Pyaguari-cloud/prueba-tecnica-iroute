namespace Commerce.Api.Models;

/// <summary>
/// Registro de la tabla dbo.commerce_quarantine: los mismos datos del
/// comercio más el motivo por el que fue rechazado.
/// </summary>
public class QuarantineRecord : CommerceRecord
{
    /// <summary>Identificador del registro en cuarentena.</summary>
    public int Id { get; set; }

    /// <summary>Id que tenía el registro en la tabla commerce.</summary>
    public int CommerceId { get; set; }

    /// <summary>Razón (o razones separadas por " | ") del rechazo.</summary>
    public string Motivo { get; set; } = string.Empty;

    /// <summary>Fecha y hora en que el registro se movió a cuarentena.</summary>
    public DateTime QuarantinedAt { get; set; }
}